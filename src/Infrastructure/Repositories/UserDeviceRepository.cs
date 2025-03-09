using Amazon.DynamoDBv2;
using Domain.Constant;
using Domain.Entities;
using Domain.Enum;
using Domain.Repositories;
using Infrastructure.Repositories.Base;

namespace Infrastructure.Repositories;

public class UserDeviceRepository : DynamoRepository, IUserDeviceRepository
{
    public UserDeviceRepository(IAmazonDynamoDB dynamoDb)
        : base(dynamoDb) { }

    protected override string GetTableName() => TableNames.TableName;

    public async Task<bool> SaveOrUpdateDeviceAsync(
        string userId,
        string fcmToken,
        string? apnToken,
        CancellationToken cancellationToken = default
    )
    {
        var currentUtcTime = DateTime.UtcNow;

        var existingDevice = await GetDeviceAsync(fcmToken, cancellationToken);
        if (existingDevice != null)
        {
            var oldUserId = existingDevice.UserId;
            if (!string.IsNullOrEmpty(oldUserId) && oldUserId != userId)
            {
                // Remove old mapping if the userId has changed
                await DeleteUserDeviceTokenAsync(oldUserId, fcmToken, cancellationToken);
            }
        }

        var userDeviceEntity = new UserDeviceMapEntity { UserId = userId, TokenValue = fcmToken };
        var userDeviceSaved = await SaveAsync(userDeviceEntity, cancellationToken);

        var deviceEntity = new DeviceEntity
        {
            FcmToken = fcmToken,
            ApnToken = apnToken,
            UserId = userId,
            CreatedUtc = existingDevice?.CreatedUtc ?? currentUtcTime,
            UpdatedUtc = currentUtcTime,
        };
        var deviceSaved = await SaveAsync(deviceEntity, cancellationToken);

        return userDeviceSaved && deviceSaved;
    }

    public async Task<DeviceEntity?> GetDeviceAsync(
        string token,
        CancellationToken cancellationToken = default
    )
    {
        return await GetAsync<DeviceEntity>(DeviceEntity.GetPk(), token, cancellationToken);
    }

    public async Task<DeviceEntity?> GetUserActiveDeviceAsync(
        string userId,
        CancellationToken cancellationToken = default
    )
    {
        var userDevices = await GetAllAsync<UserDeviceMapEntity>(
            UserDeviceMapEntity.GetPk(userId),
            cancellationToken
        );

        var deviceTokens = userDevices
            .Select(x => new DeviceEntity { FcmToken = x.TokenValue })
            .ToList();

        var devices = await BatchGetAsync(deviceTokens, cancellationToken);
        return devices.OrderByDescending(x => x.CreatedUtc).FirstOrDefault();
    }

    public async Task<bool> DeleteUserDeviceTokenAsync(
        string userId,
        string fcmToken,
        CancellationToken cancellationToken = default
    )
    {
        var deleteUserDeviceTask = DeleteAsync(
            UserDeviceMapEntity.GetPk(userId),
            fcmToken,
            cancellationToken
        );

        var deleteDeviceTask = DeleteAsync(DeviceEntity.GetPk(), fcmToken, cancellationToken);

        await Task.WhenAll(deleteUserDeviceTask, deleteDeviceTask);
        return true;
    }
}
