using GoCare.Abstractions;

using Microsoft.Extensions.Logging;

namespace GoCare.Infrastructure;

// Stub: stampa nel log invece di inviare una vera push. Nessun dispositivo è ancora
// registrabile (manca POST /devices), quindi oggi non esistono comunque DeviceToken da usare.
public sealed class ConsolePushSender(ILogger<ConsolePushSender> logger) : IPushSender
{
    public Task SendAsync(string deviceToken, string title, string body, CancellationToken ct = default)
    {
        logger.LogInformation("[PUSH a {DeviceToken}] {Title}\n{Body}", deviceToken, title, body);
        return Task.CompletedTask;
    }
}
