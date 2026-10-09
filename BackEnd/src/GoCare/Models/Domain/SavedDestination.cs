namespace GoCare.Models.Domain;

public sealed class SavedDestination
{
    // Costruttore vuoto privato: lo usa SOLO EF per materializzare dal DB
    // (crea l'oggetto vuoto e riempie le proprietà, anche le { get; } via backing field).
    private SavedDestination() { }

    public SavedDestination(Guid id, Guid personId, string placeName, Address savedAddress, string? note = null)
    {
        Id = id;
        PersonId = personId;
        PlaceName = placeName;
        SavedAddress = savedAddress;
        Note = note;
    }

    public Guid Id { get; }
    public Guid PersonId { get; }
    public string PlaceName { get; private set; } = null!;
    public Address SavedAddress { get; private set; } = null!;
    public string? Note { get; private set; }

    public void UpdateDetails(string placeName, Address savedAddress, string? note)
    {
        PlaceName = placeName;
        SavedAddress = savedAddress;
        Note = note;
    }
}
