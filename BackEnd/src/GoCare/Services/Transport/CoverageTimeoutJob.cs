using GoCare.Data;
using GoCare.Models.Enums;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace GoCare.Services.Transport;

// PA-01: job periodico che dichiara "non coperta" una richiesta ancora in attesa quando
// mancano meno di CoverageOptions.NotCoveredThresholdHours ore alla partenza e nessuna
// associazione l'ha ancora accettata. L'altro trigger (tutte le candidate rifiutano) scatta
// invece nel servizio di rifiuto (UC 6.4, non ancora scritto).
public sealed class CoverageTimeoutJob(
    IServiceScopeFactory scopeFactory,
    IOptions<CoverageOptions> options,
    ILogger<CoverageTimeoutJob> logger) : BackgroundService
{
    private static readonly TimeSpan PollingInterval = TimeSpan.FromMinutes(15);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(PollingInterval);

        // Primo giro subito all'avvio, non dopo i primi 15 minuti: altrimenti una richiesta
        // che scade mentre il server è spento/riavviato resterebbe "in attesa" più del dovuto.
        do
        {
            try
            {
                await RunOnceAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // un giro fallito non deve fermare il job: si riprova al prossimo tick
                logger.LogError(ex, "CoverageTimeoutJob: errore durante l'elaborazione.");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task RunOnceAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<GoCareDbContext>();
        var notifications = scope.ServiceProvider.GetRequiredService<NotificationDispatcher>();

        var now = DateTimeOffset.UtcNow;
        var threshold = TimeSpan.FromHours(options.Value.NotCoveredThresholdHours);

        // pre-filtro lato DB sulla sola data: evita di scorrere tutte le richieste pendenti
        var candidates = await db.TransportRequests
            .Where(t => t.RequestStatus == ETripRequestStatus.Pending && t.DepartureDateHour - now < threshold)
            .ToListAsync(ct);

        foreach (var transport in candidates)
        {
            var candidateCount = await db.TransportRequestCandidates.CountAsync(c => c.TransportRequestId == transport.Id, ct);
            var rejectionCount = await db.TransportRequestRejections.CountAsync(r => r.TransportRequestId == transport.Id, ct);

            if (!CoverageEvaluator.IsNotCovered(
                    transport.RequestStatus, transport.DepartureDateHour, candidateCount, rejectionCount, now, threshold))
                continue;

            transport.RequestNotCovered(now);

            await notifications.DispatchAsync(
                ENotificationSubject.Person,
                transport.RequestedById,
                ENotificationType.RequestNotCovered,
                "Nessuna associazione disponibile",
                "Non è stato possibile trovare un'associazione per il tuo trasporto in tempo utile. Puoi provare a inviare una nuova richiesta per un altro giorno o orario.",
                ENotificationChannel.Push | ENotificationChannel.Email,
                transport.Id,
                ct);
        }

        if (candidates.Count > 0)
            await db.SaveChangesAsync(ct);
    }
}
