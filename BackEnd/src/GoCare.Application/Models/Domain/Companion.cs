namespace GoCare.Application.Models.Domain;

public sealed class Companion
{
    private Companion() { } // costruttore vuoto: EF (materializzazione dal DB)

    public Companion(Guid id, Guid transportRequestId, string name, string surname, string relationship, string phone)
    {
        Id = id;
        TransportRequestId = transportRequestId;
        Name = name;
        Surname = surname;
        Relationship = relationship;
        Phone = phone;
    }

    public Guid Id { get; }
    public Guid TransportRequestId { get; }
    public string Name { get; } = null!;
    public string Surname { get; } = null!;
    public string Relationship { get; } = null!;
    public string Phone { get; } = null!;
}
