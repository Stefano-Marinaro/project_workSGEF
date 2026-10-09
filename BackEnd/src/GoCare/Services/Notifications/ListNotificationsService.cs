using GoCare.Data;
using GoCare.Dtos.Notifications.Responses;
using GoCare.Models.Enums;

using Microsoft.EntityFrameworkCore;

namespace GoCare.Services.Notifications;

public sealed class ListNotificationsService(GoCareDbContext db)
{
    public async Task<List<NotificationResponse>> ListAsync(
        ENotificationSubject subjectType, Guid subjectId, CancellationToken ct)
    {
        return await db.Notifications
            .Where(n => n.SubjectType == subjectType && n.SubjectId == subjectId)
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new NotificationResponse(
                n.Id, n.Type, n.Title, n.Body, n.RelatedEntityId, n.CreatedAt, n.ReadAt))
            .ToListAsync(ct);
    }
}
