namespace GoCare.Shared.Errors;

public sealed class NotFoundException(string message) : DomainException(message);

