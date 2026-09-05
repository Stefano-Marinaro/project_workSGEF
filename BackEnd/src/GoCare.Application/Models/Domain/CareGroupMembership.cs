using GoCare.Application.Models.Enums;

namespace GoCare.Application.Models.Domain;


public sealed class CareGroupMembership(Guid careGroupId, Guid personId, EGroupAdminRole adminRole, EMembershipRole role, EInvitationGroupStatus status, DateTimeOffset createdAt)
{
    public Guid CareGroupId { get; } = careGroupId;
    public Guid PersonId { get; } = personId;
    public EGroupAdminRole AdminRole { get; private set; } = adminRole;
    public EMembershipRole Role { get; private set; } = role;
    public EInvitationGroupStatus Status { get; private set; } = status;
    public string? InvitationEmail { get; private set; }
    public string? InvitationToken { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; } = createdAt;
    public DateTimeOffset? RespondedAt { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }
}
