using Domain.Entities;

namespace Domain.Dto.User;

public record UserSettingsDto(bool IsCallDisabled);

public static class UserSettingsDtoMapper
{
    public static UserSettingsDto ToDto(this UserSettingsEntity entity)
    {
        return new UserSettingsDto(IsCallDisabled: entity.IsCallDisabled);
    }

    public static void UpdateEntity(this UserSettingsEntity entity, UserSettingsDto dto)
    {
        entity.IsCallDisabled = dto.IsCallDisabled;
    }
}
