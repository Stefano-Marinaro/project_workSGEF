using GoCare.Models.Enums;

namespace GoCare.Dtos.Transport.Responses;

public sealed record OperatorTripStatusResponse(
    ETripTransitionStatus? CurrentStatus, // null = non ancora iniziato
    ETripTransitionStatus? NextStatus,    // stato che il pulsante "avanza" porterebbe a registrare
    bool IsSuspended,
    bool CanSuspend,
    bool CanResume,
    bool IsCompleted);
