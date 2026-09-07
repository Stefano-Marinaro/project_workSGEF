using GoCare.Application.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoCare.Application.Data.Auth.Configurations;

public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");
        builder.HasKey(token => token.Id);
        builder.Property(token => token.Token).IsRequired().HasMaxLength(512);
        builder.Property(token => token.UserAgent).HasMaxLength(512);
        builder.Property(token => token.IpAddress).HasMaxLength(64);
        builder.HasIndex(token => token.Token).IsUnique();
        builder.HasIndex(token => new { token.AccountId, token.ExpiresAt });
        builder.HasIndex(token => token.DeletedAt);
        builder.HasOne<Account>().WithMany().HasForeignKey(token => token.AccountId)
            .HasConstraintName("fk_refresh_tokens_accounts_account_id")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
