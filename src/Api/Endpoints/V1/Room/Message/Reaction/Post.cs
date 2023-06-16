using Api.Endpoints.V1.Models.Room.Message;
using Api.Infrastructure.Context;
using Api.Infrastructure.Contract;
using Domain.Entities;
using Domain.Events.Contracts;
using Domain.Events.Room;
using Domain.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints.V1.Room.Message.Reaction
{
    public class Post : IEndpoint
    {
        private static async Task<IResult> Handler([FromRoute] string id,
            [FromRoute] string messageId,
            [FromBody] ReactionRequestModel request,
            [FromServices] IApiContext apiContext,
            [FromServices] IRoomRepository roomRepository,
            [FromServices] IMessageRepository messageRepository,
            [FromServices] IEventPublisher eventPublisher,
            CancellationToken cancellationToken)
        {
            var room = await roomRepository.GetRoomAsync(id, cancellationToken);
            if (room == null)
            {
                return Results.NotFound();
            }

            if (!room.Attenders.Contains(apiContext.CurrentUserId))
            {
                return Results.Forbid();
            }

            var message = await messageRepository.GetMessageAsync(id, messageId, cancellationToken);
            if (message == null)
            {
                return Results.NotFound();
            }

            if (message.MessageReactions.Any(q => q.UserId == apiContext.CurrentUserId && q.Reaction == request.Reaction))
            {
                return Results.Ok();
            }

            var utcNow = DateTime.UtcNow;

            message.MessageReactions.Add(new MessageEntity.MessageReactionDataModel
            {
                Reaction = request.Reaction,
                UserId = apiContext.CurrentUserId,
                Time = utcNow
            });
            await messageRepository.SaveMessageAsync(message, cancellationToken);

            await eventPublisher.PublishAsync(new RoomChangedEvent
            {
                RoomId = room.Id,
                ActivityAt = utcNow
            }, cancellationToken);
            return Results.Ok();
        }

        public void MapEndpoint(IEndpointRouteBuilder endpoints)
        {
            endpoints.MapPost("/v1/rooms/{id}/messages/{messageId}/reactions", Handler)
                .Produces(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status403Forbidden)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .WithTags("Room");
        }
    }
}