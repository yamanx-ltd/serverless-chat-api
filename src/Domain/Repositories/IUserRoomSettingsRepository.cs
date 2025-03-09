using Domain.Entities;

namespace Domain.Repositories;

public interface IUserRoomSettingsRepository
{
    Task<bool> SaveAsync(
        UserRoomSettingsEntity entity,
        CancellationToken cancellationToken = default
    );

    Task<UserRoomSettingsEntity?> GetAsync(
        string roomId,
        string userId,
        CancellationToken cancellationToken = default
    );
}
