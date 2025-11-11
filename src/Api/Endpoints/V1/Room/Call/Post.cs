using System.Net;
using Api.Infrastructure.Context;
using Api.Infrastructure.Contract;
using Domain.Dto.Notifier;
using Domain.Entities;
using Domain.Repositories;
using Domain.Services;
using Google.Apis.Util;
using Infrastructure.Models;
using Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints.V1.Room.Call;

public class Post : IEndpoint
{
    private static async Task<IResult> Handler(
        [FromRoute] string id,
        [FromBody] NewCallRequest request,
        [FromServices] IApiContext apiContext,
        [FromServices] IRoomRepository roomRepository,
        [FromServices] IAgoraService agoraService,
        [FromServices] IUserDeviceRepository userDeviceRepository,
        [FromServices] INotificationService notificationService,
        [FromServices] IErrorMessageBuilder errorMessageBuilder,
        [FromServices] IUserRoomSettingsRepository roomSettingsRepository,
        [FromServices] IUserSettingsRepository userSettingsRepository,
        [FromServices] IUserBanRepository banRepository,
        [FromQuery] bool useAppleSandbox = false,
        CancellationToken cancellationToken = default
    )
    {
        var room = await roomRepository.GetRoomAsync(id, cancellationToken);
        if (room == null)
        {
            return Results.Problem(
                errorMessageBuilder.BuildProblemDetailsAsync(
                    new ServiceResponse(HttpStatusCode.NotFound)
                )
            );
        }

        if (!room.IsAttender(apiContext.CurrentUserId))
            return Results.Forbid();

        if (room.VideoCall.IsAlive())
        {
            return Results.Problem(errorMessageBuilder.BuildProblemDetailsAsync("CallInProgress"));
        }

        var callees = room.Attenders.Where(userId => userId != apiContext.CurrentUserId).ToList();

        if (callees.Count != 1)
        {
            // We are not supporting group calls
            return Results.Problem(
                errorMessageBuilder.BuildProblemDetailsAsync("CallNotSupported")
            );
        }

        var callee = callees[0];

        var roomCallReceiverSetting = await roomSettingsRepository.GetAsync(
            room.Id,
            callee,
            cancellationToken
        );

        if (roomCallReceiverSetting is not { IsCallEnabled: true })
        {
            return Results.Problem(
                errorMessageBuilder.BuildProblemDetailsAsync(
                    "CallDisabled",
                    HttpStatusCode.Forbidden
                )
            );
        }
        
        var callReceiverSetting = await userSettingsRepository.GetUserSettingsAsync(
            callee,
            cancellationToken
        );

        if (callReceiverSetting is not { IsCallDisabled: false })
        {
            return Results.Problem(
                errorMessageBuilder.BuildProblemDetailsAsync(
                    "CallDisabled",
                    HttpStatusCode.Forbidden
                )
            );
        }

        var otherAttender = room.Attenders.FirstOrDefault(q => q != apiContext.CurrentUserId);
        if (otherAttender != null)
        {
            var banInfo = await banRepository.GetBannedInfoAsync(
                apiContext.CurrentUserId,
                otherAttender,
                cancellationToken
            );

            if (banInfo.Any())
            {
                return Results.Problem(
                    errorMessageBuilder.BuildProblemDetailsAsync(
                        "ThisPersonBannedYou",
                        HttpStatusCode.Forbidden
                    )
                );
            }
        }

        room.VideoCall = new()
        {
            CalledAt = DateTime.UtcNow,
            LastBeatAt = DateTime.UtcNow,
            Attenders =
            [
                new RoomEntity.VideoCallAttenderDataModel
                {
                    UserId = apiContext.CurrentUserId,
                    IsCreator = true,
                },
            ]
        };
        await roomRepository.SaveRoomAsync(room, cancellationToken);
        
        var token = agoraService.GenerateToken(id);
        var callSignal = new CallSignalModel
        {
            CallerName = request.CallerName,
            VideoSDkToken = token,
            RoomId = room.Id,
            SignalType = CallSignalModel.CallSignalType.CallInitiated,
        };

        await notificationService.SendCallSignalAsync(
            callSignal,
            callees,
            useAppleSandbox,
            cancellationToken
        );

        return Results.Ok(token);
    }

    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/v1/rooms/{id}/calls", Handler).Produces<string>().WithTags("Call");
    }
}

public record NewCallRequest(string CallerName);
