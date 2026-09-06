namespace GoCare.Application.Models.Domain;

public sealed class TransportRequestCandidate(Guid transportRequestId, Guid associationId) // lista congelata delle associazioni candidate al match geografico (PA-05).
{
    public Guid TransportRequestId { get; } = transportRequestId;
    public Guid AssociationId { get; } = associationId;

}
