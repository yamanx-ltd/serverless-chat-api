using System.Net;
using Api.Infrastructure.Context;
using Api.Infrastructure.Contract;
using Domain.Dto.Call;
using Domain.Dto.Notifier;
using Domain.Entities;
using Domain.Repositories;
using Domain.Services;
using Infrastructure.Models;
using Infrastructure.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints.V1.Room.Call;

public class Put : IEndpoint
{
    private static async Task<IResult> Handler(
        [FromRoute] string id,
        [FromBody] UpdateCallStatusModel updateCallStatus,
        [FromServices] IApiContext apiContext,
        [FromServices] IRoomRepository roomRepository,
        [FromServices] IAgoraService agoraService,
        [FromServices] IMessageService messageService,
        [FromServices] INotificationService notificationService,
        [FromServices] IErrorMessageBuilder errorMessageBuilder,
        [FromQuery] bool useAppleSandbox = false,
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

        if (!room.VideoCall.IsAlive())
            return Results.Ok();

        var token = agoraService.GenerateToken(id);
        var callSignal = new CallSignalModel
        {
            RoomId = room.Id,
            SignalType = updateCallStatus.CallStatus,
            VideoSDkToken = token,
        };

        var creator = room
            .VideoCall.Attenders.Where(attender => attender.IsCreator)
            .Select(x => x.UserId)
            .ToList();

        var recipients = new List<string>();
        if (callSignal.ShouldSendToCaller)
        {
            recipients.AddRange(creator);
        }

        if (callSignal.ShouldSendToCallee)
        {
            var callees = room
                .Attenders.Where(userId => userId != creator.FirstOrDefault())
                .ToList();
            recipients.AddRange(callees);
        }

        await notificationService.SendCallSignalAsync(
            callSignal,
            recipients,
            useAppleSandbox,
            cancellationToken
        );

        switch (updateCallStatus.CallStatus)
        {
            case CallSignalModel.CallSignalType.Rejected
            or CallSignalModel.CallSignalType.Canceled
            or CallSignalModel.CallSignalType.Ended:
                await messageService.SendCallMessageAsync(
                    room,
                    callSignal,
                    isMissed: updateCallStatus.CallStatus != CallSignalModel.CallSignalType.Ended,
                    cancellationToken
                );
                room.VideoCall = new RoomEntity.VideoCallDataModel();
                break;
            case CallSignalModel.CallSignalType.Accepted:
                room.VideoCall.LastBeatAt = DateTime.UtcNow;
                room.VideoCall.AnsweredAt = DateTime.UtcNow;
                room.VideoCall.Attenders.Add(
                    new RoomEntity.VideoCallAttenderDataModel
                    {
                        UserId = currentUserId,
                        IsCreator = false,
                    }
                );
                break;
        }
        await roomRepository.SaveRoomAsync(room, cancellationToken);

        return Results.Ok();
    }

    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPut("/v1/rooms/{id}/calls", Handler).WithTags("Call");
    }
}
