namespace GoCare.Application.Models.Domain;

public sealed class Companion(
    Guid id, Guid transportRequestId, string name, string surname, string relationship, string phone )
{
    public Guid Id { get; } = id;
    public Guid TransportRequestId { get; } = transportRequestId;
    public string Name { get; } = name;
    public string Surname { get; } = surname;
    public string Relationship { get; } = relationship;
    public string Phone { get; } = phone;
}
