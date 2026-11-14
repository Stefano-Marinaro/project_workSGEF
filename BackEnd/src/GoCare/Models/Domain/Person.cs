namespace GoCare.Models.Domain;

public sealed class Person
{
    private Person() { } // costruttore vuoto: EF (materializzazione dal DB)

    // I recapiti e la data di nascita possono essere completati dopo la registrazione.
    public Person(Guid id, string email, string? name = null, string? surname = null)
    {
        Id = id;
        Email = email;
        Name = name;
        Surname = surname;
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

    // Modifica completa del profilo (PUT): anche l'indirizzo, che può essere rimosso (null).
    // L'e-mail non passa di qui: si cambia solo con il flusso di conferma (ChangeEmail).
    public void UpdateProfile(string name, string surname, DateOnly birthDate, string phone, Address? homeAddress)
    {
        Name = name;
        Surname = surname;
        BirthDate = birthDate;
        Phone = phone;
        HomeAddress = homeAddress;
    }

    public void ChangeEmail(string newEmail)
    {
        Email = newEmail;
    }

    // un profilo anonimizzato non ha più i dati personali: non si può ripristinare
    public void Restore()
    {
        if (AnonymizedAt is not null)
            throw new InvalidOperationException("Il profilo è stato anonimizzato e non può essere ripristinato.");

        DeletedAt = null;
    }
}
