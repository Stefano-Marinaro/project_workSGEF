using GoCare.Application.Models.Enums;

namespace GoCare.Application.Models.Auth;

public sealed class Account
{
    private Account() { }

    public Account(
        Guid id,
        string email,
        string passwordHash,
        EAccountRole role,
        DateTimeOffset createdAt)
    {
        Id = id;
        Email = email;
        PasswordHash = passwordHash;
        Role = role;
        CreatedAt = createdAt;
    }

    public Guid Id { get; }
    public string Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public EAccountRole Role { get; } 
    public EAccountStatus Status { get; private set; } = EAccountStatus.Unverified;
    public bool CanLogIn => Status == EAccountStatus.Active; // controlla chi può loggare: solo chi ha account attivo.
    public DateTimeOffset CreatedAt { get; } 
    public DateTimeOffset? EmailVerifiedAt { get; private set; }
    public DateTimeOffset? SuspendedAt { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }

    public void Verify(DateTimeOffset at)
    {
        if (Status is not EAccountStatus.Unverified)
            throw new InvalidOperationException("Impossibile verificare questo account.");

        Status = EAccountStatus.Active;
        EmailVerifiedAt = at;
    }

    public void ChangePassword(string newPasswordHash)
    {
        if (Status is EAccountStatus.Deleted)
            throw new InvalidOperationException("L'account non esiste.");

        PasswordHash = newPasswordHash;
    }

    public void ChangeEmail(string newEmail, DateTimeOffset verifiedAt)
    {
        if (Status is EAccountStatus.Deleted)
            throw new InvalidOperationException("L'account non esiste.");

        Email = newEmail;
        EmailVerifiedAt = verifiedAt;
    }

    public void Suspend(DateTimeOffset at)
    {
        if (Status is EAccountStatus.Deleted || Status is EAccountStatus.Suspended)
            throw new InvalidOperationException("L'account è stato sospeso o non esiste");

        Status = EAccountStatus.Suspended;
        SuspendedAt = at;   
    }

    public void Reinstate()
    {
        if (Status is not EAccountStatus.Suspended)
            throw new InvalidOperationException("Impossibile riattivare un account non sospeso.");

        Status = EAccountStatus.Active;
        SuspendedAt = null;
    }

    public void MarkDeleted(DateTimeOffset at)
    {
        if (Status is EAccountStatus.Deleted)
            throw new InvalidOperationException("L'account già è stato eliminato.");

        Status = EAccountStatus.Deleted;
        DeletedAt = at;
    }
}

