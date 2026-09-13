using GoCare.Application.Models.Enums;

namespace GoCare.Application.Models.Domain;

public sealed class Notification
{
    private Notification() { } // costruttore vuoto: EF (materializzazione dal DB)

    public Notification(
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
        Id = id;
        SubjectType = subjectType;
        SubjectId = subjectId;
        Type = type;
        Title = title;
        Body = body;
        Channels = channels;
        RelatedEntityId = relatedEntityId;
        CreatedAt = createdAt;
    }

    public Guid Id { get; }
    public ENotificationSubject SubjectType { get; }
    public Guid SubjectId { get; }
    public ENotificationType Type { get; }
    public string Title { get; } = null!;
    public string Body { get; } = null!;
    public ENotificationChannel Channels { get; }
    public Guid? RelatedEntityId { get; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset? ReadAt { get; private set; }

    public void MarkRead(DateTimeOffset at) => ReadAt ??= at; //  if (ReadAt is null) ReadAt = at;
}
