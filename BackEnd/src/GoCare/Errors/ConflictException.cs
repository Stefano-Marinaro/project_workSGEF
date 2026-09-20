namespace GoCare.Errors;

public sealed class ConflictException(string message) : DomainException(message);

