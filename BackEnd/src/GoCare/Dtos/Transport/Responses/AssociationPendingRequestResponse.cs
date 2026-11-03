using GoCare.Models.Enums;

namespace GoCare.Dtos.Transport.Responses;

// Dati minimi della richiesta in fase di valutazione (UC 6.1, 6.2), prima dell'accettazione
// e del pagamento: niente contatti né indirizzo completo (PA-02). Dopo l'accettazione
// l'associazione vede invece il trasporto per intero tramite AssociationTransportResponse.
public sealed record AssociationPendingRequestResponse(
    Guid Id,
    string BeneficiaryName,
    string StartStreet,
    DateTimeOffset DepartureDateHour,
    ETripDirection TripDirection,
    double DistanceToReachKm,
    double TripDistanceKm);
