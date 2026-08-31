namespace GoCare.Shared.Errors;

public sealed class ValidationException(IReadOnlyDictionary<string, string[]> errors) : DomainException("Uno o più campi non sono validi.")
{
    public IReadOnlyDictionary<string, string[]> Errors { get; } = errors;
}
