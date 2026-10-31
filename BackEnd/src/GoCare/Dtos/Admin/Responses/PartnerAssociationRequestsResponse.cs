using GoCare.Models.Enums;
using GoCare.Pagination;

namespace GoCare.Dtos.Admin.Responses;

public sealed record PartnerAssociationRequestsResponse(
    PartnerAssociationIndicators Indicators,       // calcolati su tutto il periodo, senza il filtro per esito
    PagedResult<PartnerAssociationRequestItem> Requests);

public sealed record PartnerAssociationIndicators(
    int Received,
    int Accepted,                     // comprese quelle poi disdette
    int Declined,
    int CancelledAfterAcceptance,
    double? AverageAcceptanceMinutes); // dalla creazione della richiesta all'accettazione; null se nessuna

// nessun recapito personale: solo dati di servizio
public sealed record PartnerAssociationRequestItem(
    Guid TransportId,
    DateTimeOffset CreatedAt,
    DateTimeOffset DepartureDateHour,
    ETripType TripType,
    string StartCity,
    ETripRequestStatus RequestStatus,
    ECandidateOutcome Outcome,
    string? RejectionReason);
