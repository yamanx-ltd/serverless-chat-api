using Domain.Dto.Notifier;
using Domain.Entities;
using Domain.Enum;
using Domain.Extensions;
using Domain.Repositories;
using Domain.Services;

namespace Infrastructure.Services;

public class MessageService : IMessageService
{
    private readonly IMessageRepository _messageRepository;

    public MessageService(IMessageRepository messageRepository)
    {
        _messageRepository = messageRepository;
    }

    public async Task SendCallMessageAsync(
        RoomEntity room,
        CallSignalModel callSignal,
        bool isMissed,
        CancellationToken cancellationToken
    )
    {
        var callerId = room.VideoCall.Attenders.FirstOrDefault(q => q.IsCreator)?.UserId;
        if (callerId == null)
            return;

        var utcNow = DateTime.UtcNow;
        var messageId = utcNow.ToUnixTimeMilliseconds().ToString();
        var messageEntity = new MessageEntity
        {
            Id = messageId,
            RoomId = room.Id,
            CreatedAt = utcNow,
            MessageReactions = new List<MessageEntity.MessageReactionDataModel>(),
            MessageStatus = room
                .Attenders.Where(q => q != callerId)
                .Select(q => new MessageEntity.MessageStatusDataModel
                {
                    Status = MessageStatus.Delivered,
                    CreatedUtc = utcNow,
                    TargetId = q,
                })
                .ToList(),
            SenderId = callerId,
            IsSystemMessage = true,
            SystemMessage = new MessageEntity.SystemMessagePayload
            {
                Type = MessageEntity.SystemMessageType.VideoCall,
                AdditionalData = new MessageEntity.VideoCallStatusDataModel
                {
                    IsMissed = isMissed,
                    CallerId = callerId,
                    CallEndTime = room.VideoCall.AnsweredAt ?? utcNow,
                    CallStartTime = room.VideoCall.CalledAt,
                },
            },
        };

        await _messageRepository.SaveMessageAsync(messageEntity, cancellationToken);
    }
}
