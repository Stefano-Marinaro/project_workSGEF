using GoCare.Data;
using GoCare.Dtos.Notifications.Responses;
using GoCare.Models.Enums;

using Microsoft.EntityFrameworkCore;

namespace GoCare.Services.Notifications;

public sealed class NotificationCountersService(GoCareDbContext db)
{
    public async Task<NotificationCountersResponse> GetCountersAsync(
        ENotificationSubject subjectType, Guid subjectId, CancellationToken ct)
    {
        var query = db.Notifications.Where(n => n.SubjectType == subjectType && n.SubjectId == subjectId);

        var total = await query.CountAsync(ct);
        var unread = await query.CountAsync(n => n.ReadAt == null, ct);

        return new NotificationCountersResponse(unread, total);
    }
}
