using GoCare.Application.Models.Auth;

using Microsoft.EntityFrameworkCore;

namespace GoCare.Application.Data;

public sealed class AuthDbContext(DbContextOptions<AuthDbContext> options) : DbContext(options)
{
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<EmailVerificationToken> EmailVerificationTokens => Set<EmailVerificationToken>();
    public DbSet<FailedLoginAttempt> FailedLoginAttempts => Set<FailedLoginAttempt>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<Enum>().HaveConversion<string>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Account>(account =>
        {
            account.HasKey(a => a.Id);
            account.Property(a => a.Email).HasMaxLength(255);
            account.HasIndex(a => a.Email).IsUnique();
        });

        modelBuilder.Entity<EmailVerificationToken>(token =>
        {
            token.HasKey(t => t.Id);
            token.Property(t => t.Token).HasMaxLength(255);

            token.HasOne<Account>()
                .WithMany()
                .HasForeignKey(t => t.AccountId)
                .OnDelete(DeleteBehavior.Cascade);

            token.HasIndex(t => t.Token).IsUnique();
        });

        modelBuilder.Entity<PasswordResetToken>(passToken =>
        {
            passToken.HasKey(p => p.Id);
            passToken.Property(t => t.Token).HasMaxLength(255);

            passToken.HasOne<Account>()
                .WithMany()
                .HasForeignKey(p => p.AccountId)
                .OnDelete(DeleteBehavior.Cascade);

            passToken.HasIndex(t => t.Token).IsUnique();
        });

        modelBuilder.Entity<RefreshToken>(refreshToken =>
        {
            refreshToken.HasKey(p => p.Id);
            refreshToken.Property(t => t.Token).HasMaxLength(255);

            refreshToken.HasOne<Account>()
                .WithMany()
                .HasForeignKey(p => p.AccountId)
                .OnDelete(DeleteBehavior.Cascade);

            refreshToken.HasIndex(t => t.Token).IsUnique();
        });

        modelBuilder.Entity<FailedLoginAttempt>(attempt =>
        {
            attempt.HasKey(a => a.Id);
            attempt.Property(a => a.Email).HasMaxLength(255);
            attempt.Property(a => a.IpAddress).HasMaxLength(45);

            attempt.HasIndex(a => new { a.Email, a.AttemptedAt }); 
        });
    }
}

