namespace GoCare.Errors;

public sealed class ForbiddenException(string message) : DomainException(message);
