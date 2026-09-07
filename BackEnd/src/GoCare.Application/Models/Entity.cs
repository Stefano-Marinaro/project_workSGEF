namespace GoCare.Application.Models;

/// <summary>Proprietà tecniche comuni alle entità persistite.</summary>
public abstract class Entity
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    // Token gestito dall'applicazione: PostgreSQL non espone un SQL Server rowversion.
    public Guid RowVersion { get; set; } = Guid.CreateVersion7();
}

public interface ISoftDeletable
{
    DateTimeOffset? DeletedAt { get; set; }
}
