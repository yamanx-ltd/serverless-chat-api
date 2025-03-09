using Domain.Dto.Notifier;
using Domain.Entities;

namespace Domain.Services;

public interface IMessageService
{
    Task SendCallMessageAsync(
        RoomEntity room,
        CallSignalModel callSignal,
        bool isMissed,
        CancellationToken cancellationToken
    );
}
