using Amazon.DynamoDBv2;
using Domain.Constant;
using Domain.Entities;
using Domain.Entities.Base;
using Domain.Extensions;
using Domain.Repositories;
using Infrastructure.Repositories.Base;

namespace Infrastructure.Repositories;

public class UserRoomSettingsRepository : DynamoRepository, IUserRoomSettingsRepository
{
    public UserRoomSettingsRepository(IAmazonDynamoDB dynamoDb)
        : base(dynamoDb) { }

    protected override string GetTableName() => TableNames.TableName;

    public Task<bool> SaveAsync(
        UserRoomSettingsEntity entity,
        CancellationToken cancellationToken = default
    )
    {
        return base.SaveAsync(entity, cancellationToken);
    }

    public Task<UserRoomSettingsEntity?> GetAsync(
        string roomId,
        string userId,
        CancellationToken cancellationToken = default
    )
    {
        return base.GetAsync<UserRoomSettingsEntity>(
            UserRoomSettingsEntity.GetPk(roomId),
            userId,
            cancellationToken
        );
    }
}
