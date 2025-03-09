using Domain.Entities;

namespace Domain.Dto.User;

public record UserRoomSettingsDto(bool IsCallEnabled, bool IsCallModalViewed);

public static class RoomSettingsDtoMapper
{
    public static UserRoomSettingsDto ToDto(this UserRoomSettingsEntity entity)
    {
        return new UserRoomSettingsDto(entity.IsCallEnabled, entity.IsCallModalViewed);
    }

    public static void UpdateEntity(this UserRoomSettingsEntity entity, UserRoomSettingsDto dto)
    {
        entity.IsCallModalViewed = dto.IsCallModalViewed;
        entity.IsCallEnabled = dto.IsCallEnabled;
        entity.UpdatedUtc = DateTime.UtcNow;
    }
}
