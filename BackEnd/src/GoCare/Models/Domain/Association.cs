using GoCare.Models.Enums;

namespace GoCare.Models.Domain;

public sealed class Association
{
    // Costruttore senza parametri, privato: lo usa SOLO EF per materializzare dal DB.
    // EF crea l'oggetto vuoto e poi riempie ogni proprietà (anche { get; } scrivendone il backing field).
    private Association() { }

    // costruttore minimo: solo quello che si ha alla registrazione (email+password).
    // il resto del profilo arriva dopo, tramite CompleteProfile, come passo verso l'accreditamento.
    public Association(Guid id, string email)
    {
        Id = id;
        Email = email;
    }

    public Guid Id { get; }
    public string? Name { get; private set; }
    public Address? Headquarter { get; private set; }
    public List<string>? Phones { get; private set; }
    public string Email { get; private set; } = null!;
    public string? AvailabilityHours { get; private set; }
    public EAccreditationStatus Status { get; private set; } = EAccreditationStatus.Pending;
    public DateTimeOffset? DeletedAt { get; private set; }
    public List<string>? CoveredProvinces { get; private set; }

    public bool IsProfileComplete =>
        Name is not null && Headquarter is not null && Phones is not null && CoveredProvinces is not null;

    public bool CanOperate =>
        IsProfileComplete && Status is EAccreditationStatus.Accredited;

    public void CompleteProfile(string name, Address headquarter, List<string> phones, List<string> coveredProvinces)
    {
        Name = name;
        Headquarter = headquarter;
        Phones = phones;
        CoveredProvinces = coveredProvinces;
    }

    public void Accredit()
    {
        if (Status is not EAccreditationStatus.Pending)
            throw new InvalidOperationException("Solo un'associazione in attesa può essere accreditata.");

        Status = EAccreditationStatus.Accredited;
    }

    public void Reject()
    {
        if (Status is not EAccreditationStatus.Pending)
            throw new InvalidOperationException("Solo un'associazione in attesa può essere rifiutata.");

        Status = EAccreditationStatus.Rejected;
    }

    public void ChangeEmail(string newEmail)
    {
        Email = newEmail;
    }
}
