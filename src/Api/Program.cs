using System.Globalization;
using System.Reflection;
using Amazon.DynamoDBv2;
using Amazon.Extensions.Configuration.SystemsManager;
using Amazon.SimpleNotificationService;
using Amazon.SQS;
using Api.Extensions;
using Api.Infrastructure.Context;
using Api.Infrastructure.Middleware;
using Domain.Events;
using Domain.Events.Contracts;
using Domain.Options;
using Domain.Repositories;
using Domain.Services;
using dotAPNS;
using dotAPNS.AspNetCore;
using FirebaseAdmin;
using FluentValidation;
using FluentValidation.AspNetCore;
using Google.Apis.Auth.OAuth2;
using Infrastructure;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddSystemsManager(config =>
{
    config.Path = "/chat-api";
    config.ParameterProcessor = new JsonParameterProcessor();
    config.ReloadAfter = TimeSpan.FromMinutes(5);
    config.Optional = true;
});

var googleCredentialJson = builder
    .Configuration.GetSection("GoogleCredential")
    .GetChildren()
    .ToDictionary(x => x.Key, x => x.Value)
    .ToJson();
FirebaseApp.Create(new AppOptions { Credential = GoogleCredential.FromJson(googleCredentialJson) });

builder.Services.AddApns();
builder.Services.Configure<EventBusSettings>(builder.Configuration.GetSection("EventBusSettings"));
builder.Services.Configure<ApnsJwtOptions>(builder.Configuration.GetSection("AppleApnOptions"));
builder.Services.Configure<AwsWebSocketAdapterConfig>(
    builder.Configuration.GetSection("AwsWebSocketAdapterConfig")
);
builder.Services.Configure<ApiKeyValidationSettings>(
    builder.Configuration.GetSection("ApiKeyValidationSettings")
);
builder.Services.Configure<AgoraSettings>(builder.Configuration.GetSection("AgoraSettings"));

// Add services to the container.
builder.Services.AddHttpContextAccessor();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// builder.Logging.ClearProviders();
// Serilog configuration
// var logger = new LoggerConfiguration()
//     .Enrich.WithProperty("Application", "Chat")
//     .WriteTo.Console(new JsonFormatter())
//     .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
//     .MinimumLevel.Override("AWSSDK", LogEventLevel.Warning)
//     .MinimumLevel.Override("System.", LogEventLevel.Warning)
//     .CreateLogger();

// Register Serilog
// TODO
// builder.Logging.AddSerilog(logger);

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] { new CultureInfo("en-US"), new CultureInfo("tr-TR") };
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
    options.RequestCultureProviders.Insert(
        0,
        new CustomRequestCultureProvider(context =>
        {
            var languages = context.Request.Headers["x-culture"].ToString();
            var currentLanguage = languages.Split(',').FirstOrDefault();
            var defaultLanguage = string.IsNullOrEmpty(currentLanguage) ? "en-US" : currentLanguage;
            if (!supportedCultures.Where(s => s.Name.Equals(defaultLanguage)).Any())
            {
                defaultLanguage = "en-US";
            }

            return Task.FromResult(new ProviderCultureResult(defaultLanguage, defaultLanguage));
        })
    );
});
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(option =>
{
    // Add security definition for x-user-id header
    option.AddSecurityDefinition(
        "UserId",
        new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Name = "x-user-id",
            Type = SecuritySchemeType.ApiKey,
            Description = "Provide the user ID to authenticate the request.",
        }
    );

    option.AddSecurityDefinition(
        "ApiKey",
        new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Name = "x-api-key",
            Type = SecuritySchemeType.ApiKey,
            Description = "Provide the api key",
        }
    );

    option.AddSecurityDefinition(
        "Culture",
        new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Name = "x-culture",
            Type = SecuritySchemeType.ApiKey,
            Description = "Provide culture to localize the response.",
        }
    );

    option.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "ApiKey",
                    },
                },
                []
            },
        }
    );

    option.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "UserId",
                    },
                },
                []
            },
        }
    );

    option.CustomOperationIds(e =>
    {
        // Extract and join route values
        var routeValues = string.Join(
            "_",
            e.ActionDescriptor.RouteValues.OrderByDescending(o => o.Key).Select(i => i.Value)
        );

        // Extract the namespace from the MethodInfo in EndpointMetadata
        var methodInfo = e.ActionDescriptor.EndpointMetadata.OfType<MethodInfo>().FirstOrDefault();
        var namespaceName = methodInfo?.DeclaringType?.Namespace?.Split('.').Last() ?? "Default";

        // Return the custom operation ID including the namespace and route values
        return $"{namespaceName}_{routeValues}";
    });
});

var option = builder.Configuration.GetAWSOptions();
builder.Services.AddDefaultAWSOptions(option);

builder.Services.AddAWSService<IAmazonDynamoDB>();
builder.Services.AddAWSService<IAmazonSQS>();
builder.Services.AddAWSService<IAmazonSimpleNotificationService>();
builder.Services.AddAWSLambdaHosting(
    Environment.GetEnvironmentVariable("ApiGatewayType") == "RestApi"
        ? LambdaEventSource.RestApi
        : LambdaEventSource.HttpApi
);

var type = typeof(Program);
var assemblyName = new AssemblyName(type.GetTypeInfo().Assembly.FullName);
builder.Services.AddSingleton<IStringLocalizerFactory, ResourceManagerStringLocalizerFactory>();
builder.Services.AddSingleton<IStringLocalizer>(provider =>
    provider.GetService<IStringLocalizerFactory>().Create("Resource", assemblyName.Name)
);

builder.Services.AddScoped<IClearRoomRepository, ClearRoomRepository>();
builder.Services.AddScoped<IDeletedMessageRepository, DeletedMessageRepository>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();
builder.Services.AddScoped<IRoomRepository, RoomRepository>();
builder.Services.AddScoped<IRoomNotificationRepository, RoomNotificationRepository>();
builder.Services.AddScoped<IUserBanRepository, UserBanRepository>();
builder.Services.AddScoped<IUserSettingsRepository, UserSettingsRepository>();
builder.Services.AddScoped<IUserDeviceRepository, UserDeviceRepository>();
builder.Services.AddScoped<IUserRoomRepository, UserRoomRepository>();
builder.Services.AddScoped<IUserRoomSettingsRepository, UserRoomSettingsRepository>();
builder.Services.AddScoped<IRoomLastActivityRepository, RoomLastActivityRepository>();
builder.Services.AddScoped<IApiContext, ApiContext>();
builder.Services.AddScoped<IEventPublisher, EventPublisher>();
builder.Services.AddScoped<IPubSubServices, PubSubService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IAgoraService, AgoraService>();
builder.Services.AddScoped<IEventBusManager, EventBusManager>();
builder.Services.AddScoped<IErrorMessageBuilder, ErrorMessageBuilder>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<IProblematicImageRepository, ProblematicImageRepository>();
builder.Services.AddScoped<ApiKeyValidatorMiddleware>();
var assemblies = GetAssembly();
foreach (var assembly in assemblies)
{
    builder.Services.AddClassesAsImplementedInterface(
        assembly,
        typeof(IConsumer<>),
        ServiceLifetime.Transient
    );
}

var app = builder.Build();

var localizationOptions = app.Services.GetService<IOptions<RequestLocalizationOptions>>();
app.UseRequestLocalization(localizationOptions.Value);

app.UseMiddleware<ApiKeyValidatorMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.EnablePersistAuthorization();
    });
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler(exceptionHandlerApp =>
        exceptionHandlerApp.Run(async context => await Results.Problem().ExecuteAsync(context))
    );
}

app.UseHttpsRedirection();
app.MapEndpointsCore(AppDomain.CurrentDomain.GetAssemblies());

app.Run();

static IEnumerable<Assembly> GetAssembly()
{
    yield return typeof(Program).Assembly;
    yield return typeof(IConsumer<>).Assembly;
}
