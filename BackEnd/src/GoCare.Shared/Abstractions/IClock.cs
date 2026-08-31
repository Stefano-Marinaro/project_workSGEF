namespace GoCare.Shared.Abstractions;


/// Astrazione dell'orologio: si inietta al posto di DateTimeOffset.UtcNow,
/// così i test possono fissare un istante preciso.

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
