using System.Text.Json.Serialization;
using Domain.Entities.Base;

namespace Domain.Entities;

public class UserSettingsEntity : IEntity
{
    [JsonPropertyName("pk")]
    public string Pk => GetPk();

    [JsonPropertyName("sk")]
    public string Sk => UserId;

    [JsonPropertyName("userId")]
    public string UserId { get; set; } = default!;

    [JsonPropertyName("isCallDisabled")]
    public bool IsCallDisabled { get; set; } = false;

    public static string GetPk() => "userSettings";
    
    public void UpdateFrom(UserSettingsEntity userSettings)
    {
        IsCallDisabled = userSettings.IsCallDisabled;
    }
    
    public static UserSettingsEntity Create(UserSettingsEntity userSettings)
    {
        return new UserSettingsEntity
        {
            UserId = userSettings.UserId,
            IsCallDisabled = userSettings.IsCallDisabled
        };
    }
}
