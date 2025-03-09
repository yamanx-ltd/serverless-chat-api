using Domain.Options;
using Domain.Services;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services;

public class AgoraService(IOptionsSnapshot<AgoraSettings> agoraSettings) : IAgoraService
{
    private readonly AgoraSettings _agoraSettings = agoraSettings.Value;

    public string GenerateToken(string channelId)
    {
        return new AgoraIO.Rtc.RtcTokenBuilder().BuildToken(
            _agoraSettings.AppId,
            _agoraSettings.AppCertificate,
            channelName: channelId,
            publisher: true,
            privilegeTs: 0
        );
    }
}
