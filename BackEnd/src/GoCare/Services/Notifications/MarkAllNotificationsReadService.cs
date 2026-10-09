using GoCare.Data;
using GoCare.Models.Enums;

using Microsoft.EntityFrameworkCore;

namespace GoCare.Services.Notifications;

public sealed class MarkAllNotificationsReadService(GoCareDbContext db)
{
    public async Task MarkAllReadAsync(ENotificationSubject subjectType, Guid subjectId, CancellationToken ct)
    {
        var unread = await db.Notifications
            .Where(n => n.SubjectType == subjectType && n.SubjectId == subjectId && n.ReadAt == null)
            .ToListAsync(ct);

        if (unread.Count == 0)
            return;

        var now = DateTimeOffset.UtcNow;
        foreach (var notification in unread)
            notification.MarkRead(now);

        await db.SaveChangesAsync(ct);
    }
}
