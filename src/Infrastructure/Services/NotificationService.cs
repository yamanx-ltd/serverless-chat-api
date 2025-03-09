using Domain.Dto.Notifier;
using Domain.Entities;
using Domain.Enum;
using Domain.Repositories;
using Domain.Services;
using dotAPNS;
using dotAPNS.AspNetCore;
using FirebaseAdmin.Messaging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services;

public class NotificationService(
    IUserDeviceRepository userDeviceRepository,
    IApnsService apnsService,
    IOptionsSnapshot<ApnsJwtOptions> apnsJwtOptions
) : INotificationService
{
    private readonly ApnsJwtOptions _apnsJwtOptions = apnsJwtOptions.Value;

    public async Task SendCallSignalAsync(
        CallSignalModel callSignal,
        List<string> users,
        bool useAppleSandbox = false,
        CancellationToken cancellationToken = default
    )
    {
        foreach (var userId in users)
        {
            var userDevice = await userDeviceRepository.GetUserActiveDeviceAsync(
                userId,
                cancellationToken
            );

            if (userDevice == null)
                continue;

            // Send call signal to iOS device if the signal is CallInitiated and the device has an APN token
            if (
                callSignal.SignalType == CallSignalModel.CallSignalType.CallInitiated
                && userDevice.ApnToken != null
            )
            {
                await SendIosCallSignalAsync(
                    callSignal,
                    userDevice,
                    useAppleSandbox,
                    cancellationToken
                );
                continue;
            }

            await SendAndroidCallSignalAsync(callSignal, userDevice, cancellationToken);
        }
    }

    private async Task SendAndroidCallSignalAsync(
        CallSignalModel callSignal,
        DeviceEntity device,
        CancellationToken cancellationToken = default
    )
    {
        var message = new Message
        {
            Token = device.FcmToken,
            Android = new AndroidConfig { Priority = Priority.High },
            Data = callSignal.ToDictionary(),
        };

        try
        {
            await FirebaseMessaging.DefaultInstance.SendAsync(message, cancellationToken);
        }
        catch (FirebaseMessagingException ex)
        {
            if (ex.MessagingErrorCode == MessagingErrorCode.Unregistered)
            {
                await userDeviceRepository.DeleteUserDeviceTokenAsync(
                    userId: device.UserId,
                    fcmToken: device.FcmToken,
                    cancellationToken
                );
            }
        }
        catch (Exception)
        {
            Console.WriteLine("Failed to send FCM");
        }
    }

    private async Task SendIosCallSignalAsync(
        CallSignalModel callSignal,
        DeviceEntity device,
        bool useAppleSandbox = false,
        CancellationToken cancellationToken = default
    )
    {
        var push = new ApplePush(ApplePushType.Voip)
            .AddContentAvailable()
            .AddVoipToken(device.ApnToken)
            .AddCustomProperty("uuid", Guid.NewGuid());

        var callSignalDict = callSignal.ToDictionary();
        foreach (var kvp in callSignalDict)
        {
            push.AddCustomProperty(kvp.Key, kvp.Value);
        }

        var response = await apnsService.SendPush(push, _apnsJwtOptions, useAppleSandbox);

        if (
            !response.IsSuccessful
            && response.Reason is ApnsResponseReason.Unregistered or ApnsResponseReason.ExpiredToken
        )
        {
            await userDeviceRepository.DeleteUserDeviceTokenAsync(
                userId: device.UserId,
                fcmToken: device.FcmToken,
                cancellationToken
            );
        }
    }
}
