using System.Net;
using Infrastructure.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public interface IErrorMessageBuilder
{
    ProblemDetails BuildProblemDetailsAsync(ServiceResponse serviceResponse);
    ProblemDetails BuildProblemDetailsAsync(
        string message,
        HttpStatusCode errorCode = HttpStatusCode.BadRequest
    );
}

public class ErrorMessageBuilder : IErrorMessageBuilder
{
    private readonly IStringLocalizer _localizer;
    private readonly ILogger<ErrorMessageBuilder> _logger;

    public ErrorMessageBuilder(IStringLocalizer localizer, ILogger<ErrorMessageBuilder> logger)
    {
        _localizer = localizer;
        _logger = logger;
    }

    public ProblemDetails BuildProblemDetailsAsync(ServiceResponse serviceResponse)
    {
        var errorCode = (int)serviceResponse.StatusCode;
        var errorMessage = _localizer.GetString($"Error_{errorCode}");
        if (string.IsNullOrWhiteSpace(errorMessage))
        {
            errorMessage = _localizer.GetString("Error_Unknown");
        }

        var logLevel = errorCode switch
        {
            >= 500 => LogLevel.Error,
            _ => LogLevel.Warning,
        };

        _logger.Log(
            logLevel,
            "Error: {ServiceResponseStatusCode} - {ErrorMessage}",
            serviceResponse.StatusCode,
            serviceResponse.Error?.Detail ?? errorMessage
        );

        var errorDetail = errorMessage.ToString();
        if (serviceResponse.StatusCode == HttpStatusCode.BadRequest)
        {
            errorDetail = serviceResponse.Error?.Detail ?? errorMessage;
        }

        return new ProblemDetails
        {
            Status = errorCode,
            Title = errorDetail,
            Detail = errorDetail,
        };
    }

    public ProblemDetails BuildProblemDetailsAsync(
        string message,
        HttpStatusCode errorCode = HttpStatusCode.BadRequest
    )
    {
        var errorMessage = _localizer.GetString(message);

        return new ProblemDetails
        {
            Status = (int)errorCode,
            Title = errorMessage,
            Detail = errorMessage,
        };
    }
}
