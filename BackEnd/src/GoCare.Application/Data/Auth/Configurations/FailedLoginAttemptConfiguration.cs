using GoCare.Application.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoCare.Application.Data.Auth.Configurations;

public sealed class FailedLoginAttemptConfiguration : IEntityTypeConfiguration<FailedLoginAttempt>
{
    public void Configure(EntityTypeBuilder<FailedLoginAttempt> builder)
    {
        builder.ToTable("failed_login_attempts");
        builder.HasKey(attempt => attempt.Id);
        builder.Property(attempt => attempt.Email).IsRequired().HasMaxLength(320);
        builder.Property(attempt => attempt.IpAddress).HasMaxLength(64);
        builder.HasIndex(attempt => new { attempt.Email, attempt.AttemptedAt });
        builder.HasIndex(attempt => new { attempt.AccountId, attempt.AttemptedAt });
        builder.HasIndex(attempt => attempt.DeletedAt);
        builder.HasOne<Account>().WithMany().HasForeignKey(attempt => attempt.AccountId)
            .HasConstraintName("fk_failed_login_attempts_accounts_account_id")
            .OnDelete(DeleteBehavior.SetNull);
    }
}
