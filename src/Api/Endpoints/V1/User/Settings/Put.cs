using Api.Infrastructure.Context;
using Api.Infrastructure.Contract;
using Domain.Dto.User;
using Domain.Entities;
using Domain.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints.V1.User.Settings
{
    public class Put : IEndpoint
    {
        private static async Task<IResult> Handler(
            [FromBody] UserSettingsDto userSettingsDto,
            [FromServices] IApiContext apiContext,
            [FromServices] IUserSettingsRepository userSettingsRepository,
            CancellationToken cancellationToken
        )
        {
            var settings = new UserSettingsEntity { UserId = apiContext.CurrentUserId };
            settings.UpdateEntity(userSettingsDto);

            await userSettingsRepository.SaveOrUpdateUserSettingsAsync(settings, cancellationToken);

            return Results.Ok();
        }

        public void MapEndpoint(IEndpointRouteBuilder endpoints)
        {
            endpoints
                .MapPut("v1/user/settings", Handler)
                .Produces<UserSettingsDto>()
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status500InternalServerError)
                .WithTags("User");
        }
    }
}
