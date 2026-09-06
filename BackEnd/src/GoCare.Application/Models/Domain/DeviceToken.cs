using GoCare.Application.Models.Enums;

namespace GoCare.Application.Models.Domain;

public sealed class DeviceToken(
    Guid id,
    ENotificationSubject subjectType,
    Guid subjectId,
    string pushToken,
    EDevicePlatform platform,
    DateTimeOffset createdAt)
{
    public Guid Id { get; } = id;
    public ENotificationSubject SubjectType { get; } = subjectType;
    public Guid SubjectId { get; } = subjectId;
    public string PushToken { get; } = pushToken;
    public EDevicePlatform Platform { get; } = platform;
    public DateTimeOffset CreatedAt { get; } = createdAt;
    public DateTimeOffset? DeactivatedAt { get; private set; }

    public void Deactivate(DateTimeOffset at) => DeactivatedAt ??= at;
}
