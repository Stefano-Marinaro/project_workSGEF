namespace GoCare.Dtos.Transport.Requests;

public sealed record DeleteTransportRequest(Guid TransportId, Guid DeletedBy);

