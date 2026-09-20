namespace GoCare.Models.Domain;

public sealed class Person
{
    private Person() { } // costruttore vuoto: EF (materializzazione dal DB)

    // costruttore minimo: solo quello che si ha alla registrazione (email+password).
    // il resto del profilo arriva dopo, tramite CompleteProfile.
    public Person(Guid id, string email)
    {
        Id = id;
        Email = email;
    }

    public Guid Id { get; }
    public string? Name { get; private set; }
    public string? Surname { get; private set; }
    public DateOnly? BirthDate { get; private set; }
    public string Email { get; private set; } = null!;
    public string? Phone { get; private set; }
    public Address? HomeAddress { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; } // Offset = istante preciso univoco nel tempo a prescindere dai fusi orario
    public DateTimeOffset? AnonymizedAt { get; private set; }

    public bool IsProfileComplete =>
        Name is not null && Surname is not null && BirthDate is not null && Phone is not null;

    public void CompleteProfile(string name, string surname, DateOnly birthDate, string phone)
    {
        Name = name;
        Surname = surname;
        BirthDate = birthDate;
        Phone = phone;
    }
}
