using Api.Infrastructure.Context;
using Api.Infrastructure.Contract;
using Domain.Dto.User;
using Domain.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints.V1.Room.Settings
{
    public class Get : IEndpoint
    {
        private static async Task<IResult> Handler(
            [FromRoute] string roomId,
            [FromServices] IApiContext apiContext,
            [FromServices] IUserRoomSettingsRepository roomSettingsRepository,
            [FromServices] IRoomRepository roomRepository,
            CancellationToken cancellationToken
        )
        {
            var room = await roomRepository.GetRoomAsync(roomId, cancellationToken);
            if (room == null)
            {
                return Results.NotFound();
            }

            var entity = await roomSettingsRepository.GetAsync(
                roomId,
                apiContext.CurrentUserId,
                cancellationToken
            );

            if (entity == null)
                return Results.Ok(
                    new UserRoomSettingsDto(IsCallEnabled: false, IsCallModalViewed: false)
                );

            return Results.Ok(entity.ToDto());
        }

        public void MapEndpoint(IEndpointRouteBuilder endpoints)
        {
            endpoints
                .MapGet("v1/users/rooms/{roomId}/settings", Handler)
                .Produces<UserRoomSettingsDto>()
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .WithTags("Room");
        }
    }
}
