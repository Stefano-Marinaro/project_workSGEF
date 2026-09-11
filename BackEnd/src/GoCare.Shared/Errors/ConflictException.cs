namespace GoCare.Shared.Errors;

public sealed class ConflictException(string message) : DomainException(message);

