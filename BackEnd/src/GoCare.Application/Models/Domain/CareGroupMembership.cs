using GoCare.Application.Models.Enums;

namespace GoCare.Application.Models.Domain;

public sealed class CareGroupMembership
{
    private CareGroupMembership(
        Guid careGroupId,
        Guid personId,
        EGroupAdminRole adminRole,
        EGroupRole role,
        EInvitationGroupStatus status,
        string? invitationEmail,
        string? invitationToken,
        DateTimeOffset createdAt)
    {
        CareGroupId = careGroupId;
        PersonId = personId;
        AdminRole = adminRole;
        Role = role;
        Status = status;
        InvitationEmail = invitationEmail;
        InvitationToken = invitationToken;
        CreatedAt = createdAt;
    }
    public Guid CareGroupId { get; }
    public Guid PersonId { get; }
    public EGroupAdminRole AdminRole { get; private set; }
    public EGroupRole Role { get; private set; }
    public EInvitationGroupStatus Status { get; private set; }
    public string? InvitationEmail { get; private set; }
    public string? InvitationToken { get; private set; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset? RespondedAt { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }

    public static CareGroupMembership CreateInvitation(
        Guid careGroupId,
        Guid personId,
        EGroupRole role,
        string invitationEmail,
        string invitationToken,
        DateTimeOffset at
        )
    {
        if (string.IsNullOrWhiteSpace(invitationEmail))
            throw new ArgumentException("Email di invito mancante.", nameof(invitationEmail));

        if (string.IsNullOrWhiteSpace(invitationToken))
            throw new ArgumentException("Token di invito mancante.", nameof(invitationToken));

        var membership = new CareGroupMembership(
            careGroupId,
            personId,
            adminRole: EGroupAdminRole.Member,
            role: role,
            status: EInvitationGroupStatus.Pending,
            invitationEmail: invitationEmail,
            invitationToken: invitationToken,
            createdAt: at
            );

        return membership;
    }

    public static CareGroupMembership CreateOwner( Guid careGroupId, Guid personId, DateTimeOffset at) //Creazione ADMIN gruppo: gli assegniamo questi 3 valori come input, il resto glieli diamo di default
    {
        var membership = new CareGroupMembership(
            careGroupId,
            personId,
            adminRole: EGroupAdminRole.Admin,
            role: EGroupRole.Caregiver,
            status: EInvitationGroupStatus.Accepted,
            invitationEmail: null,
            invitationToken: null,
            createdAt: at);

        membership.RespondedAt = at;

        return membership;
    }
    
    public void Accept(DateTimeOffset at)
    {
        if (Status != EInvitationGroupStatus.Pending)
            throw new InvalidOperationException("Per accettare un invito deve avere stato 'Pending'");

        Status = EInvitationGroupStatus.Accepted;
        RespondedAt = at;
    }

    public void Decline(DateTimeOffset at)
    {
        if (Status != EInvitationGroupStatus.Pending)
            throw new InvalidOperationException("Per rifiutare un invito deve avere stato 'Pending'");

        Status = EInvitationGroupStatus.Refused;
        RespondedAt = at;
    }

    public void Revoke(DateTimeOffset at)
    {
        if (Status != EInvitationGroupStatus.Pending)
            throw new InvalidOperationException("Per ritirare un invito deve avere stato 'Pending'");

        Status = EInvitationGroupStatus.Revoked;
        DeletedAt = at;
    }

    public void ChangeRole(EGroupRole role)
    {
        if (Status != EInvitationGroupStatus.Accepted)
            throw new InvalidOperationException("Non è possibile cambiare ruolo su un invito non accettato.");

        Role = role;
    }

    public void ChangeAdminRole(EGroupAdminRole adminRole)
    {
        if (Status != EInvitationGroupStatus.Accepted)
            throw new InvalidOperationException("Si può cambiare il ruolo solo di un membro con invito accettato.");

        AdminRole = adminRole;
    }

    public void RemoveFromGroup(DateTimeOffset at)
    {
        if (Status != EInvitationGroupStatus.Accepted)
            throw new InvalidOperationException("Si può rimuovere solo un membro con invito accettato.");

        if (DeletedAt is not null)
            throw new InvalidOperationException("Membro già rimosso.");

        DeletedAt = at;
    }
}
    
    
