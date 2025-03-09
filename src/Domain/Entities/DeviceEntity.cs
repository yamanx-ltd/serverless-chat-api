using System.Text.Json.Serialization;
using Domain.Entities.Base;
using Domain.Enum;
using Domain.Extensions;
using Domain.Repositories;

namespace Domain.Entities;

public class DeviceEntity : IEntity
{
    [JsonPropertyName("pk")]
    public string Pk => "device";

    [JsonPropertyName("sk")]
    public string Sk => FcmToken;

    [JsonPropertyName("userId")]
    public string UserId { get; set; } = default!;

    [JsonPropertyName("FcmToken")]
    public string FcmToken { get; set; } = default!;

    [JsonPropertyName("ApnToken")]
    // This token is only available in iOS
    public string? ApnToken { get; set; } = default!;

    [JsonPropertyName("createdUtc")]
    public DateTime CreatedUtc { get; set; }

    [JsonPropertyName("updatedUtc")]
    public DateTime UpdatedUtc { get; set; }

    public static string GetPk() => "device";
}
