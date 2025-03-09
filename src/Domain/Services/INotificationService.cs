using Domain.Dto.Notifier;

namespace Domain.Services;

public interface INotificationService
{
    Task SendCallSignalAsync(
        CallSignalModel callSignal,
        List<string> users,
        bool useAppleSandbox = false,
        CancellationToken cancellationToken = default
    );
}
