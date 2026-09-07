using GoCare.Application.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoCare.Application.Data.Auth.Configurations;

public sealed class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("accounts");
        builder.HasKey(account => account.Id);
        builder.Property(account => account.Email).HasMaxLength(320).IsRequired();
        builder.HasIndex(account => account.Email).IsUnique();
        builder.Property(account => account.PasswordHash).IsRequired();
        builder.Property(account => account.Role).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(account => account.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.HasIndex(account => account.DeletedAt);
    }
}
