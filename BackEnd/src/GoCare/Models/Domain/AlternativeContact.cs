namespace GoCare.Models.Domain;

public sealed class AlternativeContact
{
    private AlternativeContact() { } // costruttore vuoto: EF (materializzazione dal DB)

    public AlternativeContact(Guid id, Guid transportRequestId, string phone, string? note)
    {
        Id = id;
        TransportRequestId = transportRequestId;
        Phone = phone;
        Note = note;
    }

    public Guid Id { get; }
    public Guid TransportRequestId { get; }
    public string Phone { get; } = null!;
    public string? Note { get; }
}
