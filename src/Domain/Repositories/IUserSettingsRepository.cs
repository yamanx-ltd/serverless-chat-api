using Domain.Entities;

namespace Domain.Repositories
{
    public interface IUserSettingsRepository
    {
        Task<bool> SaveOrUpdateUserSettingsAsync(
            UserSettingsEntity userSettings,
            CancellationToken cancellationToken = default
        );

        Task<UserSettingsEntity> GetUserSettingsAsync(
            string userId,
            CancellationToken cancellationToken = default
        );
    }
}
