using System.Net;
using Api.Infrastructure.Context;
using Api.Infrastructure.Contract;
using Domain.Repositories;
using Infrastructure.Models;
using Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints.V1.Room.Call.Beat;

public class Post : IEndpoint
{
    private static async Task<IResult> Handler(
        [FromRoute] string id,
        [FromServices] IApiContext apiContext,
        [FromServices] IRoomRepository roomRepository,
        [FromServices] IErrorMessageBuilder errorMessageBuilder,
        CancellationToken cancellationToken = default
    )
    {
        var currentUserId = apiContext.CurrentUserId;
        var room = await roomRepository.GetRoomAsync(id, cancellationToken);
        if (room is null)
        {
            return Results.Problem(
                errorMessageBuilder.BuildProblemDetailsAsync(
                    new ServiceResponse(HttpStatusCode.NotFound)
                )
            );
        }

        if (!room.IsAttender(currentUserId))
            return Results.Forbid();

        if (room.VideoCall.Attenders.Count > 0)
            room.VideoCall.LastBeatAt = DateTime.UtcNow;
        await roomRepository.SaveRoomAsync(room, cancellationToken);

        return Results.Ok();
    }

    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/v1/rooms/{id}/calls/beat", Handler).WithTags("Call");
    }
}
