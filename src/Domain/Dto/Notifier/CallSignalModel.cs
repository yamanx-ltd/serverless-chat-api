namespace Domain.Dto.Notifier;

public class CallSignalModel
{
    public CallSignalType SignalType { get; set; }
    public string RoomId { get; set; }
    public string VideoSDkToken { get; set; }

    public string? CallerName { get; set; }

    public enum CallSignalType
    {
        // Call initiated by caller
        CallInitiated = 0,

        // Call accepted by callee
        Accepted = 1,

        // Call rejected by callee
        Rejected = 2,

        // Call ended by either party
        Ended = 3,

        // Call canceled by caller
        Canceled = 4,
    }

    public bool ShouldSendToCaller =>
        SignalType is CallSignalType.Accepted or CallSignalType.Rejected or CallSignalType.Ended;

    public bool ShouldSendToCallee =>
        SignalType
            is CallSignalType.CallInitiated
                or CallSignalType.Accepted
                or CallSignalType.Ended
                or CallSignalType.Canceled;

    public Dictionary<string, string> ToDictionary()
    {
        var dict = new Dictionary<string, string>
        {
            { "type", ((int)SignalType).ToString() },
            { "roomId", RoomId },
            { "callerName", CallerName ?? string.Empty },
            { "videoSDkToken", VideoSDkToken ?? string.Empty },
        };
        return dict;
    }
}
