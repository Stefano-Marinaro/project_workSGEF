namespace GoCare.Application.Models.Domain;

public sealed class CareGroup(Guid id, string name, Guid createdBy)
{
    public Guid Id { get; } = id;
    public string Name { get; private set; } = name;
    public string? Description { get; private set; }
    public Guid? CreatedBy { get; private set; } = createdBy;
    public DateTimeOffset? DeletedAt { get; private set; }
}
