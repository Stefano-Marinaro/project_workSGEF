namespace GoCare.Application.Models.Domain;

public sealed class TransportRequestRecipient(Guid transportRequestId, Guid associationId) // questa tabella permetterà di visualizzare il risultato del filtro geografico.
{
    public Guid TransportRquestId { get; } = transportRequestId;
    public Guid AssociationId { get; } = associationId;

}
