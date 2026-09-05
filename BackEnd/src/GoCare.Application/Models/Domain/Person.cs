namespace GoCare.Application.Models.Domain;

public sealed class Person(Guid id, string name, string surname, DateOnly birthDate, string email, string phone)
{
    public Guid Id { get; } = id;
    public string Name { get; private set; } = name;
    public string Surname { get; private set; } = surname;
    public DateOnly BirthDate { get; private set; } = birthDate;
    public string Email { get; private set; } = email;
    public string Phone { get; private set; } = phone;
    public Address? PersonAddress { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; } // Offset = istante preciso univoco nel tempo a prescindere dai fusi orario
    public DateTimeOffset? AnonimizedAt { get; private set; }

}
