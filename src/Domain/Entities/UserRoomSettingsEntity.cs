using System.Text.Json.Serialization;
using Domain.Entities.Base;

namespace Domain.Entities
{
    public class UserRoomSettingsEntity : IEntity
    {
        [JsonPropertyName("pk")]
        public string Pk => $"userRoomSettings#{RoomId}";

        [JsonPropertyName("sk")]
        public string Sk => UserId;

        [JsonPropertyName("userId")]
        public string UserId { get; set; } = default!;

        [JsonPropertyName("roomId")]
        public string RoomId { get; set; } = default!;

        [JsonPropertyName("isCallEnabled")]
        public bool IsCallEnabled { get; set; } = default!;

        [JsonPropertyName("isCallModalViewed")]
        public bool IsCallModalViewed { get; set; }

        [JsonPropertyName("createdUtc")]
        public DateTime CreatedUtc { get; set; }

        [JsonPropertyName("updatedUtc")]
        public DateTime UpdatedUtc { get; set; }

        public static string GetPk(string roomId) => $"userRoomSettings#{roomId}";
    }
}
