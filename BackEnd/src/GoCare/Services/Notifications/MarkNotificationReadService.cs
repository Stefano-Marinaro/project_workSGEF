using GoCare.Data;
using GoCare.Errors;
using GoCare.Models.Enums;

using Microsoft.EntityFrameworkCore;

namespace GoCare.Services.Notifications;

public sealed class MarkNotificationReadService(GoCareDbContext db)
{
    public async Task MarkReadAsync(
        ENotificationSubject subjectType, Guid subjectId, Guid notificationId, CancellationToken ct)
    {
        var notification = await db.Notifications.SingleOrDefaultAsync(
            n => n.Id == notificationId && n.SubjectType == subjectType && n.SubjectId == subjectId, ct)
            ?? throw new NotFoundException("Notifica non trovata.");

        notification.MarkRead(DateTimeOffset.UtcNow);

        await db.SaveChangesAsync(ct);
    }
}
