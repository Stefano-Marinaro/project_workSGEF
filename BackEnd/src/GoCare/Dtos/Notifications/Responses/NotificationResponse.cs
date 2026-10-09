using GoCare.Models.Enums;

namespace GoCare.Dtos.Notifications.Responses;

public sealed record NotificationResponse(
    Guid Id,
    ENotificationType Type,
    string Title,
    string Body,
    Guid? RelatedEntityId,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ReadAt);
