using GoCare.Abstractions;

using Microsoft.Extensions.Logging;

namespace GoCare.Infrastructure;

public sealed class ConsoleEmailSender(ILogger<ConsoleEmailSender> logger) : IEmailSender
{
    public Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default)
    {
        logger.LogInformation("[EMAIL a {To}] {Subject}\n{Body}", to, subject, htmlBody);
        return Task.CompletedTask;
    }
}
