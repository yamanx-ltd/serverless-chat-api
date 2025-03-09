using Api.Infrastructure.Context;
using Api.Infrastructure.Contract;
using Domain.Dto.User;
using Domain.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints.V1.User.Settings
{
    public class Get : IEndpoint
    {
        private static async Task<IResult> Handler(
            [FromServices] IApiContext apiContext,
            [FromServices] IUserSettingsRepository userSettingsRepository,
            CancellationToken cancellationToken
        )
        {
            var userSettings = await userSettingsRepository.GetUserSettingsAsync(
                apiContext.CurrentUserId,
                cancellationToken
            );
            return Results.Ok(userSettings.ToDto());
        }

        public void MapEndpoint(IEndpointRouteBuilder endpoints)
        {
            endpoints
                .MapGet("v1/user/settings", Handler)
                .Produces<UserSettingsDto>()
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status500InternalServerError)
                .WithTags("User");
        }
    }
}
