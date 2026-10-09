using GoCare.Data;
using GoCare.Errors;
using GoCare.Models.Enums;

using Microsoft.EntityFrameworkCore;

namespace GoCare.Services.Devices;

public sealed class UnregisterDeviceService(GoCareDbContext db)
{
    public async Task UnregisterAsync(
        ENotificationSubject subjectType, Guid subjectId, Guid deviceId, CancellationToken ct)
    {
        var device = await db.DeviceTokens.SingleOrDefaultAsync(
            d => d.Id == deviceId && d.SubjectType == subjectType && d.SubjectId == subjectId && d.DeactivatedAt == null, ct)
            ?? throw new NotFoundException("Dispositivo non trovato.");

        device.Deactivate(DateTimeOffset.UtcNow);

        await db.SaveChangesAsync(ct);
    }
}
