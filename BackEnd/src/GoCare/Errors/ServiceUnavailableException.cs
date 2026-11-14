namespace GoCare.Errors;

public sealed class ServiceUnavailableException(string message) : DomainException(message);
