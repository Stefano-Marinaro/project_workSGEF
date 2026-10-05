using GoCare.Abstractions;

using Microsoft.Extensions.Logging;

namespace GoCare.Infrastructure;

public sealed class ConsoleEmailSender(ILogger<ConsoleEmailSender> logger) : IEmailSender
{
    public Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default)
    {
        logger.LogInformation(
            "Email send requested. RecipientProvided: {RecipientProvided}, SubjectLength: {SubjectLength}, BodyLength: {BodyLength}",
            !string.IsNullOrWhiteSpace(to),
            subject?.Length ?? 0,
            htmlBody?.Length ?? 0
        );
        return Task.CompletedTask;
    }
}
