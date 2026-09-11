namespace GoCare.Application.Models.Domain;

public sealed class Person
{
    private Person() { } // costruttore vuoto: EF (materializzazione dal DB)

    public Person(Guid id, string name, string surname, DateOnly birthDate, string email, string phone)
    {
        Id = id;
        Name = name;
        Surname = surname;
        BirthDate = birthDate;
        Email = email;
        Phone = phone;
    }

    public Guid Id { get; }
    public string Name { get; private set; } = null!;
    public string Surname { get; private set; } = null!;
    public DateOnly BirthDate { get; private set; }
    public string Email { get; private set; } = null!;
    public string Phone { get; private set; } = null!;
    public Address? HomeAddress { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; } // Offset = istante preciso univoco nel tempo a prescindere dai fusi orario
    public DateTimeOffset? AnonymizedAt { get; private set; }
}
