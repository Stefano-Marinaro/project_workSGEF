namespace GoCare.Application.Models.Domain;

public sealed class SavedDestinations(Guid id, Guid personId, string placeName, Address savedAddress)
{
    public Guid Id { get; } = id;
    public Guid PersonId { get; } = personId;
    public string PlaceName { get; private set; } = placeName;
    public Address SavedAddress { get; private set; } = savedAddress;
    public string? Note { get; private set; }
}
