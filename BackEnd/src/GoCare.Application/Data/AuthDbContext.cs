using GoCare.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace GoCare.Application.Data;

public sealed class AuthDbContext(DbContextOptions<AuthDbContext> options) : GoCareDbContext(options)
{
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<EmailVerificationToken> EmailVerificationTokens => Set<EmailVerificationToken>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<FailedLoginAttempt> FailedLoginAttempts => Set<FailedLoginAttempt>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuthDbContext).Assembly,
            type => type.Namespace == "GoCare.Application.Data.Auth.Configurations");
    }
}
