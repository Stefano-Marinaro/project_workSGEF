namespace GoCare.Models.Domain;

// lista congelata delle associazioni candidate al match geografico (PA-05).
public sealed class TransportRequestCandidate
{
    private TransportRequestCandidate() { } // costruttore vuoto: EF (materializzazione dal DB)

    public TransportRequestCandidate(Guid transportRequestId, Guid associationId, double distanceToReachKm)
    {
        TransportRequestId = transportRequestId;
        AssociationId = associationId;
        DistanceToReachKm = distanceToReachKm;
    }

    public Guid TransportRequestId { get; }
    public Guid AssociationId { get; }

    // Distanza stradale dalla sede dell'associazione all'indirizzo di partenza del trasporto:
    // quanto deve spostarsi per prendere in carico l'assistito. Calcolata una sola volta alla
    // creazione della richiesta (stesso IRoutingService usato per TransportRequest.DistanceKm),
    // non al momento della lettura, per non chiamare il servizio esterno a ogni apertura della dashboard.
    public double DistanceToReachKm { get; }
}
