using GoCare.Data;
using GoCare.Models.Domain;
using GoCare.Models.Enums;

namespace GoCare.Services.Devices;

public sealed class RegisterDeviceService(GoCareDbContext db)
{
    public async Task<Guid> RegisterAsync(
        ENotificationSubject subjectType, Guid subjectId, string pushToken, EDevicePlatform platform, CancellationToken ct)
    {
        var deviceId = Guid.NewGuid();

        var device = new DeviceToken(deviceId, subjectType, subjectId, pushToken, platform, DateTimeOffset.UtcNow);

        db.DeviceTokens.Add(device);
        await db.SaveChangesAsync(ct);

        return deviceId;
    }
}
