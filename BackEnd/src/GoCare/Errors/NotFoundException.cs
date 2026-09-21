namespace GoCare.Errors;

public sealed class NotFoundException(string message) : DomainException(message);

