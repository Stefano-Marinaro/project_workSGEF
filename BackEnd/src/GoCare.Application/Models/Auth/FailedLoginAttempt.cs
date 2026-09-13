namespace GoCare.Application.Models.Auth;

public sealed class FailedLoginAttempt
{
    private FailedLoginAttempt() { } 

    public FailedLoginAttempt(Guid id, string email, DateTimeOffset attemptedAt, string? ipAddress)
    {
        Id = id;
        Email = email;
        AttemptedAt = attemptedAt;
        IpAddress = ipAddress;
    }

    public Guid Id { get; }
    public string Email { get; } = null!;  
    public DateTimeOffset AttemptedAt { get; }
    public string? IpAddress { get; }
}

