using GoCare.Application.Models.Enums;

namespace GoCare.Application.Models.Domain;

public sealed class Notification(
    Guid id,
    ENotificationSubject subjectType,
    Guid subjectId,
    ENotificationType type,
    string title,
    string body,
    ENotificationChannel channels,
    Guid? relatedEntityId,
    DateTimeOffset createdAt)
{
    public Guid Id { get; } = id;
    public ENotificationSubject SubjectType { get; } = subjectType;
    public Guid SubjectId { get; } = subjectId;
    public ENotificationType Type { get; } = type;
    public string Title { get; } = title;
    public string Body { get; } = body;
    public ENotificationChannel Channels { get; } = channels;
    public Guid? RelatedEntityId { get; } = relatedEntityId;
    public DateTimeOffset CreatedAt { get; } = createdAt;
    public DateTimeOffset? ReadAt { get; private set; }

    public void MarkRead(DateTimeOffset at) => ReadAt ??= at; //  if (ReadAt is null) ReadAt = at;
}

