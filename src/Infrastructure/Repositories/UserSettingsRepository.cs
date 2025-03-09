using Amazon.DynamoDBv2;
using Domain.Constant;
using Domain.Entities;
using Domain.Repositories;
using Infrastructure.Repositories.Base;

namespace Infrastructure.Repositories;

public class UserSettingsRepository : DynamoRepository, IUserSettingsRepository
{
    public UserSettingsRepository(IAmazonDynamoDB dynamoDb)
        : base(dynamoDb) { }

    protected override string GetTableName() => TableNames.TableName;

    public async Task<bool> SaveOrUpdateUserSettingsAsync(
        UserSettingsEntity userSettings,
        CancellationToken cancellationToken = default
    )
    {
        var existingUserSettings = await GetUserSettingsAsync(
            userSettings.UserId,
            cancellationToken
        );

        existingUserSettings.UpdateFrom(userSettings);
        return await SaveAsync(existingUserSettings, cancellationToken);
    }

    public async Task<UserSettingsEntity> GetUserSettingsAsync(
        string userId,
        CancellationToken cancellationToken = default
    )
    {
        return await GetAsync<UserSettingsEntity>(
                UserSettingsEntity.GetPk(),
                userId,
                cancellationToken
            ) ?? new UserSettingsEntity { UserId = userId, IsCallDisabled = false };
    }
}
