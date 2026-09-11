namespace GoCare.Application.Models.Domain;

// lista congelata delle associazioni candidate al match geografico (PA-05).
public sealed class TransportRequestCandidate
{
    private TransportRequestCandidate() { } // costruttore vuoto: EF (materializzazione dal DB)

    public TransportRequestCandidate(Guid transportRequestId, Guid associationId)
    {
        TransportRequestId = transportRequestId;
        AssociationId = associationId;
    }

    public Guid TransportRequestId { get; }
    public Guid AssociationId { get; }
}
