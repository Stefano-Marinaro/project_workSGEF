namespace GoCare.Application.Models;

public enum AccountRole
{
    User = 1,
    Association = 2
}

public enum AccountStatus
{
    Unverified = 1,
    Active = 2,
    PendingAccreditation = 3,
    Suspended = 4,
    Deleted = 5
}

public sealed class Account : Entity, ISoftDeletable
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public AccountRole Role { get; set; }
    public AccountStatus Status { get; set; }
    public Guid? PersonId { get; set; }
    public Guid? AssociationId { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}
