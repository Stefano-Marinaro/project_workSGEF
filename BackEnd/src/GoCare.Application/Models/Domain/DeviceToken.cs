using GoCare.Application.Models.Enums;

namespace GoCare.Application.Models.Domain;

public sealed class DeviceToken
{
    private DeviceToken() { } // costruttore vuoto: EF (materializzazione dal DB)

    public DeviceToken(
        Guid id,
        ENotificationSubject subjectType,
        Guid subjectId,
        string pushToken,
        EDevicePlatform platform,
        DateTimeOffset createdAt)
    {
        Id = id;
        SubjectType = subjectType;
        SubjectId = subjectId;
        PushToken = pushToken;
        Platform = platform;
        CreatedAt = createdAt;
    }

    public Guid Id { get; }
    public ENotificationSubject SubjectType { get; }
    public Guid SubjectId { get; }
    public string PushToken { get; } = null!;
    public EDevicePlatform Platform { get; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset? DeactivatedAt { get; private set; }

    public void Deactivate(DateTimeOffset at) => DeactivatedAt ??= at;
}
