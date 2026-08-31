namespace GoCare.Shared.Errors;

public abstract class DomainException(string message) : Exception(message);

