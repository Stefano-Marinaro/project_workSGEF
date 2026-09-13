namespace GoCare.Application.Models.Domain;

public sealed class CareGroup
{
    private CareGroup() { } // costruttore vuoto: EF (materializzazione dal DB)

    public CareGroup(Guid id, string name, Guid createdBy)
    {
        Id = id;
        Name = name;
        CreatedBy = createdBy;
    }

    public Guid Id { get; }
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public Guid CreatedBy { get; }
    public DateTimeOffset? DeletedAt { get; private set; }
}
