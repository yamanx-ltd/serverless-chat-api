using Domain.Entities.Base;

namespace Domain.Entities;

using System.Text.Json.Serialization;

public class RoomEntity : IEntity
{
    [JsonPropertyName("pk")] public string Pk => $"rooms";

    [JsonPropertyName("sk")] public string Sk => Id;

    [JsonPropertyName("id")] public string Id { get; set; } = default!;

    [JsonPropertyName("name")] public string Name { get; set; } = default!;

    [JsonPropertyName("isGroup")] public bool IsGroup { get; set; }

    [JsonPropertyName("description")] public string Description { get; set; } = default!;

    [JsonPropertyName("imageUrl")] public string ImageUrl { get; set; } = default!;

    [JsonPropertyName("attenders")] public List<string> Attenders { get; set; } = new();

    [JsonPropertyName("admins")] public List<string> Admins { get; set; } = new();

    [JsonPropertyName("lastMessageInfo")] public List<string> LastMessageInfo { get; set; } = new();

    [JsonPropertyName("typingAttenders")]
    public List<TypingAttenderDataModel> TypingAttenders { get; set; } = new();

    [JsonPropertyName("createdAt")] public DateTime CreatedAt { get; set; }

    [JsonPropertyName("lastActivityAt")] public DateTime LastActivityAt { get; set; }
    
    [JsonPropertyName("videoCall")] public VideoCallDataModel VideoCall { get; set; } = new();
    
    
    public class TypingAttenderDataModel
    {
        [JsonPropertyName("userId")] public string UserId { get; set; } = default!;

        [JsonPropertyName("typingAt")] public DateTime TypingAt { get; set; }
    }
    
    public class VideoCallDataModel
    {
        [JsonPropertyName("attenders")] public List<VideoCallAttenderDataModel> Attenders { get; set; } = new();
        
        [JsonPropertyName("calledAt")] public DateTime CalledAt { get; set; }
        
        [JsonPropertyName("answeredAt")] public DateTime? AnsweredAt { get; set; }
        
        [JsonPropertyName("lastBeatAt")] public DateTime? LastBeatAt { get; set; }
        
        public bool IsAlive()
        {
            if (LastBeatAt is null) return false;
            return DateTime.UtcNow - LastBeatAt <= TimeSpan.FromSeconds(10);
        }
    }
       
    public class VideoCallAttenderDataModel
    {
        [JsonPropertyName("userId")] public string UserId { get; set; } = default!;

        [JsonPropertyName("isCreator")] public bool IsCreator { get; set; }
    }

    public bool IsAdmin(string userId)
    {
        return Admins.Contains(userId);
    }

    public bool IsAttender(string userId)
    {
        return Attenders.Contains(userId);
    }
}