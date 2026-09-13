namespace GoCare.Shared.Errors;

public sealed class ForbiddenException(string message) : DomainException(message);
