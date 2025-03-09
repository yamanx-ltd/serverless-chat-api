using Domain.Entities;
using Domain.Enum;

namespace Domain.Repositories;

public interface IUserDeviceRepository
{
    public Task<bool> SaveOrUpdateDeviceAsync(
        string userId,
        string fcmToken,
        string? apnToken,
        CancellationToken cancellationToken = default
    );

    Task<DeviceEntity?> GetDeviceAsync(string token, CancellationToken cancellationToken = default);

    Task<DeviceEntity?> GetUserActiveDeviceAsync(
        string userId,
        CancellationToken cancellationToken = default
    );

    Task<bool> DeleteUserDeviceTokenAsync(
        string userId,
        string fcmToken,
        CancellationToken cancellationToken = default
    );
}
