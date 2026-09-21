namespace GoCare.Models.Domain;

public sealed class AssistedPerson
{
    private AssistedPerson() { } // costruttore vuoto: EF (materializzazione dal DB)

    public AssistedPerson(Guid id, string name, string surname, DateOnly birthDate, string phone, Guid createdBy)
    {
        Id = id;
        Name = name;
        Surname = surname;
        BirthDate = birthDate;
        Phone = phone;
        CreatedBy = createdBy;
    }

    public Guid Id { get; }                 // Guid proprio: nessun Account collegato, l'assistito non fa login
    public string Name { get; private set; } = null!;
    public string Surname { get; private set; } = null!;
    public DateOnly BirthDate { get; private set; }
    public string Phone { get; private set; } = null!;
    public Address? HomeAddress { get; private set; }
    public Guid CreatedBy { get; }          // Person.Id del caregiver che l'ha creato (audit)
    public DateTimeOffset? DeletedAt { get; private set; }
    public DateTimeOffset? AnonymizedAt { get; private set; }
}
