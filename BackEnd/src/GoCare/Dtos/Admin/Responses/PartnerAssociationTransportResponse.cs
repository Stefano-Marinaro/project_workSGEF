using GoCare.Models.Enums;

namespace GoCare.Dtos.Admin.Responses;

// un trasporto assegnato all'associazione, visto dall'admin: solo dati di servizio, nessun recapito personale
public sealed record PartnerAssociationTransportResponse(
    Guid Id,
    ETripType TripType,
    ETripDirection TripDirection,
    DateTimeOffset DepartureDateHour,
    ETripRequestStatus RequestStatus,
    string StartCity,
    string EndCity);
