using GoCare.Abstractions;
using GoCare.Data;
using GoCare.Models.Domain;
using GoCare.Models.Enums;

using Microsoft.EntityFrameworkCore;

namespace GoCare.Services.Transport;

// Centralizza: persiste la Notification, poi invia sui canali richiesti (Email/Push, o entrambi).
// Un canale senza destinatario disponibile (email mancante, nessun DeviceToken attivo) viene
// semplicemente saltato, non è un errore.
public sealed class NotificationDispatcher(GoCareDbContext db, IEmailSender emailSender, IPushSender pushSender)
{
    public async Task DispatchAsync(
        ENotificationSubject subjectType,
        Guid subjectId,
        ENotificationType type,
        string title,
        string body,
        ENotificationChannel channels,
        Guid? relatedEntityId,
        CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;

        var notification = new Notification(
            Guid.NewGuid(), subjectType, subjectId, type, title, body, channels, relatedEntityId, now);

        db.Notifications.Add(notification);
        await db.SaveChangesAsync(ct);

        if (channels.HasFlag(ENotificationChannel.Email))
        {
            var email = subjectType == ENotificationSubject.Person
                ? await db.Persons.Where(p => p.Id == subjectId).Select(p => p.Email).FirstOrDefaultAsync(ct)
                : await db.Associations.Where(a => a.Id == subjectId).Select(a => a.Email).FirstOrDefaultAsync(ct);

            if (email is not null)
                await emailSender.SendAsync(email, title, body, ct);
        }

        if (channels.HasFlag(ENotificationChannel.Push))
        {
            var deviceTokens = await db.DeviceTokens
                .Where(d => d.SubjectType == subjectType && d.SubjectId == subjectId && d.DeactivatedAt == null)
                .Select(d => d.PushToken)
                .ToListAsync(ct);

            foreach (var deviceToken in deviceTokens)
                await pushSender.SendAsync(deviceToken, title, body, ct);
        }
    }
}
