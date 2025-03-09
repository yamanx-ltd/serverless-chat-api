using Api.Infrastructure.Context;
using Api.Infrastructure.Contract;
using Domain.Dto.User;
using Domain.Entities;
using Domain.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints.V1.Room.Settings
{
    public class Put : IEndpoint
    {
        private static async Task<IResult> Handler(
            [FromRoute] string roomId,
            [FromBody] UserRoomSettingsDto userRoomSettingsDto,
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

            var settings = await GetOrCreateUserRoomSettingsEntity(
                roomId: roomId,
                apiContext: apiContext,
                roomSettingsRepository: roomSettingsRepository,
                cancellationToken: cancellationToken
            );

            settings.UpdateEntity(userRoomSettingsDto);

            await roomSettingsRepository.SaveAsync(settings, cancellationToken);
            return Results.Ok();
        }

        public void MapEndpoint(IEndpointRouteBuilder endpoints)
        {
            endpoints
                .MapPut("v1/users/rooms/{roomId}/settings", Handler)
                .Produces(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .WithTags("Room");
        }

        private static async Task<UserRoomSettingsEntity> GetOrCreateUserRoomSettingsEntity(
            string roomId,
            IApiContext apiContext,
            IUserRoomSettingsRepository roomSettingsRepository,
            CancellationToken cancellationToken
        )
        {
            var settings =
                await roomSettingsRepository.GetAsync(
                    roomId,
                    apiContext.CurrentUserId,
                    cancellationToken
                )
                ?? new UserRoomSettingsEntity
                {
                    RoomId = roomId,
                    UserId = apiContext.CurrentUserId,
                    IsCallEnabled = false,
                    CreatedUtc = DateTime.UtcNow,
                    UpdatedUtc = DateTime.UtcNow,
                };
            return settings;
        }
    }
}
