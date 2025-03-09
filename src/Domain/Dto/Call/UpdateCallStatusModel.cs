using Domain.Dto.Notifier;

namespace Domain.Dto.Call;

public class UpdateCallStatusModel
{
    public CallSignalModel.CallSignalType CallStatus { get; set; }
}