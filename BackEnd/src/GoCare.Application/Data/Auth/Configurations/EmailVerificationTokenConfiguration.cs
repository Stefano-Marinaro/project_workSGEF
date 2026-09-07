using GoCare.Application.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoCare.Application.Data.Auth.Configurations;

public sealed class EmailVerificationTokenConfiguration : IEntityTypeConfiguration<EmailVerificationToken>
{
    public void Configure(EntityTypeBuilder<EmailVerificationToken> builder)
    {
        builder.ToTable("email_verification_tokens");
        builder.HasKey(token => token.Id);
        builder.Property(token => token.Token).IsRequired().HasMaxLength(512);
        builder.HasIndex(token => token.Token).IsUnique();
        builder.HasIndex(token => new { token.AccountId, token.ExpiresAt });
        builder.HasIndex(token => token.DeletedAt);
        builder.HasOne<Account>().WithMany().HasForeignKey(token => token.AccountId)
            .HasConstraintName("fk_email_verification_tokens_accounts_account_id")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
