using System.Text.Json.Serialization;
using Domain.Entities.Base;


namespace Domain.Entities;

public class UserDeviceMapEntity : IEntity
{
    [JsonPropertyName("pk")]
    public string Pk => $"userDeviceMap#{UserId}";

    [JsonPropertyName("sk")]
    public string Sk => TokenValue;

    [JsonPropertyName("userId")]
    public string UserId { get; set; } = default!;

    [JsonPropertyName("tokenValue")]
    public string TokenValue { get; set; } = default!;

    public static string GetPk(string userId) => $"userDeviceMap#{userId}";
}