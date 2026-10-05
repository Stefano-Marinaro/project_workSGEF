using GoCare.Models.Enums;

namespace GoCare.Dtos.Transport.Responses;

public sealed record TimelineEntryResponse(
    ETripTransitionStatus Status,
    string OperatorLabel,
    DateTimeOffset OccurredAt);
