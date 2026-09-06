using GoCare.Application.Models.Enums;

namespace GoCare.Application.Models.Domain;

public sealed class Association
{
    // Costruttore senza parametri, privato: lo usa SOLO EF per materializzare dal DB.
    // EF crea l'oggetto vuoto e poi riempie ogni proprietà (anche { get; } scrivendone il backing field).
    private Association() { }

    public Association(Guid id, string name, Address headquarter, List<string> phones, string email, List<string> coveredProvinces)
    {
        Id = id;
        Name = name;
        Headquarter = headquarter;
        Phones = phones;
        Email = email;
        CoveredProvinces = coveredProvinces;
    }

    public Guid Id { get; }
    public string Name { get; private set; } = null!;
    public Address Headquarter { get; private set; } = null!;
    public List<string> Phones { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string? AvailabilityHours { get; private set; }
    public EAccreditationStatus Status { get; private set; } = EAccreditationStatus.Pending;
    public DateTimeOffset? DeletedAt { get; private set; }
    public List<string> CoveredProvinces { get; private set; } = null!;
}
