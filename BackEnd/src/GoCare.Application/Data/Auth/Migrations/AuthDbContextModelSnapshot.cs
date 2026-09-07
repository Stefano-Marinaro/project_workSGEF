using System;
using GoCare.Application.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable disable

namespace GoCare.Application.Data.Auth.Migrations;

[DbContext(typeof(AuthDbContext))]
public partial class AuthDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("ProductVersion", "10.0.1")
            .HasAnnotation("Relational:MaxIdentifierLength", 63);

        modelBuilder.Entity<Account>(builder =>
        {
            builder.Property(account => account.Id)
                .ValueGeneratedOnAdd()
                .HasColumnType("uuid")
                .HasColumnName("id");
            builder.Property(account => account.AssociationId).HasColumnType("uuid").HasColumnName("association_id");
            builder.Property(account => account.CreatedAt).HasColumnType("timestamp with time zone").HasColumnName("created_at");
            builder.Property(account => account.DeletedAt).HasColumnType("timestamp with time zone").HasColumnName("deleted_at");
            builder.Property(account => account.Email).IsRequired().HasMaxLength(320).HasColumnType("character varying(320)").HasColumnName("email");
            builder.Property(account => account.PasswordHash).IsRequired().HasColumnType("text").HasColumnName("password_hash");
            builder.Property(account => account.PersonId).HasColumnType("uuid").HasColumnName("person_id");
            builder.Property(account => account.Role).HasConversion<string>().IsRequired().HasMaxLength(32).HasColumnType("character varying(32)").HasColumnName("role");
            builder.Property(account => account.RowVersion).IsConcurrencyToken().HasColumnType("uuid").HasColumnName("row_version");
            builder.Property(account => account.Status).HasConversion<string>().IsRequired().HasMaxLength(32).HasColumnType("character varying(32)").HasColumnName("status");
            builder.Property(account => account.UpdatedAt).HasColumnType("timestamp with time zone").HasColumnName("updated_at");

            builder.HasKey(account => account.Id).HasName("pk_accounts");
            builder.HasIndex(account => account.DeletedAt).HasDatabaseName("ix_accounts_deleted_at");
            builder.HasIndex(account => account.Email).IsUnique().HasDatabaseName("ix_accounts_email");
            builder.HasQueryFilter(account => account.DeletedAt == null);
            builder.ToTable("accounts");
        });

        modelBuilder.Entity<EmailVerificationToken>(builder =>
        {
            builder.Property(token => token.Id).ValueGeneratedOnAdd().HasColumnType("uuid").HasColumnName("id");
            builder.Property(token => token.AccountId).HasColumnType("uuid").HasColumnName("account_id");
            builder.Property(token => token.Token).IsRequired().HasMaxLength(512).HasColumnType("character varying(512)").HasColumnName("token");
            builder.Property(token => token.ExpiresAt).HasColumnType("timestamp with time zone").HasColumnName("expires_at");
            builder.Property(token => token.UsedAt).HasColumnType("timestamp with time zone").HasColumnName("used_at");
            builder.Property(token => token.CreatedAt).HasColumnType("timestamp with time zone").HasColumnName("created_at");
            builder.Property(token => token.UpdatedAt).HasColumnType("timestamp with time zone").HasColumnName("updated_at");
            builder.Property(token => token.RowVersion).IsConcurrencyToken().HasColumnType("uuid").HasColumnName("row_version");
            builder.Property(token => token.DeletedAt).HasColumnType("timestamp with time zone").HasColumnName("deleted_at");
            builder.HasKey(token => token.Id).HasName("pk_email_verification_tokens");
            builder.HasIndex(token => new { token.AccountId, token.ExpiresAt }).HasDatabaseName("ix_email_verification_tokens_account_id_expires_at");
            builder.HasIndex(token => token.DeletedAt).HasDatabaseName("ix_email_verification_tokens_deleted_at");
            builder.HasIndex(token => token.Token).IsUnique().HasDatabaseName("ix_email_verification_tokens_token");
            builder.HasQueryFilter(token => token.DeletedAt == null);
            builder.ToTable("email_verification_tokens");
        });

        modelBuilder.Entity<PasswordResetToken>(builder =>
        {
            builder.Property(token => token.Id).ValueGeneratedOnAdd().HasColumnType("uuid").HasColumnName("id");
            builder.Property(token => token.AccountId).HasColumnType("uuid").HasColumnName("account_id");
            builder.Property(token => token.Token).IsRequired().HasMaxLength(512).HasColumnType("character varying(512)").HasColumnName("token");
            builder.Property(token => token.ExpiresAt).HasColumnType("timestamp with time zone").HasColumnName("expires_at");
            builder.Property(token => token.UsedAt).HasColumnType("timestamp with time zone").HasColumnName("used_at");
            builder.Property(token => token.CreatedAt).HasColumnType("timestamp with time zone").HasColumnName("created_at");
            builder.Property(token => token.UpdatedAt).HasColumnType("timestamp with time zone").HasColumnName("updated_at");
            builder.Property(token => token.RowVersion).IsConcurrencyToken().HasColumnType("uuid").HasColumnName("row_version");
            builder.Property(token => token.DeletedAt).HasColumnType("timestamp with time zone").HasColumnName("deleted_at");
            builder.HasKey(token => token.Id).HasName("pk_password_reset_tokens");
            builder.HasIndex(token => new { token.AccountId, token.ExpiresAt }).HasDatabaseName("ix_password_reset_tokens_account_id_expires_at");
            builder.HasIndex(token => token.DeletedAt).HasDatabaseName("ix_password_reset_tokens_deleted_at");
            builder.HasIndex(token => token.Token).IsUnique().HasDatabaseName("ix_password_reset_tokens_token");
            builder.HasQueryFilter(token => token.DeletedAt == null);
            builder.ToTable("password_reset_tokens");
        });

        modelBuilder.Entity<RefreshToken>(builder =>
        {
            builder.Property(token => token.Id).ValueGeneratedOnAdd().HasColumnType("uuid").HasColumnName("id");
            builder.Property(token => token.AccountId).HasColumnType("uuid").HasColumnName("account_id");
            builder.Property(token => token.Token).IsRequired().HasMaxLength(512).HasColumnType("character varying(512)").HasColumnName("token");
            builder.Property(token => token.ExpiresAt).HasColumnType("timestamp with time zone").HasColumnName("expires_at");
            builder.Property(token => token.RevokedAt).HasColumnType("timestamp with time zone").HasColumnName("revoked_at");
            builder.Property(token => token.UserAgent).HasMaxLength(512).HasColumnType("character varying(512)").HasColumnName("user_agent");
            builder.Property(token => token.IpAddress).HasMaxLength(64).HasColumnType("character varying(64)").HasColumnName("ip_address");
            builder.Property(token => token.CreatedAt).HasColumnType("timestamp with time zone").HasColumnName("created_at");
            builder.Property(token => token.UpdatedAt).HasColumnType("timestamp with time zone").HasColumnName("updated_at");
            builder.Property(token => token.RowVersion).IsConcurrencyToken().HasColumnType("uuid").HasColumnName("row_version");
            builder.Property(token => token.DeletedAt).HasColumnType("timestamp with time zone").HasColumnName("deleted_at");
            builder.HasKey(token => token.Id).HasName("pk_refresh_tokens");
            builder.HasIndex(token => new { token.AccountId, token.ExpiresAt }).HasDatabaseName("ix_refresh_tokens_account_id_expires_at");
            builder.HasIndex(token => token.DeletedAt).HasDatabaseName("ix_refresh_tokens_deleted_at");
            builder.HasIndex(token => token.Token).IsUnique().HasDatabaseName("ix_refresh_tokens_token");
            builder.HasQueryFilter(token => token.DeletedAt == null);
            builder.ToTable("refresh_tokens");
        });

        modelBuilder.Entity<FailedLoginAttempt>(builder =>
        {
            builder.Property(attempt => attempt.Id).ValueGeneratedOnAdd().HasColumnType("uuid").HasColumnName("id");
            builder.Property(attempt => attempt.AccountId).HasColumnType("uuid").HasColumnName("account_id");
            builder.Property(attempt => attempt.Email).IsRequired().HasMaxLength(320).HasColumnType("character varying(320)").HasColumnName("email");
            builder.Property(attempt => attempt.AttemptedAt).HasColumnType("timestamp with time zone").HasColumnName("attempted_at");
            builder.Property(attempt => attempt.IpAddress).HasMaxLength(64).HasColumnType("character varying(64)").HasColumnName("ip_address");
            builder.Property(attempt => attempt.CreatedAt).HasColumnType("timestamp with time zone").HasColumnName("created_at");
            builder.Property(attempt => attempt.UpdatedAt).HasColumnType("timestamp with time zone").HasColumnName("updated_at");
            builder.Property(attempt => attempt.RowVersion).IsConcurrencyToken().HasColumnType("uuid").HasColumnName("row_version");
            builder.Property(attempt => attempt.DeletedAt).HasColumnType("timestamp with time zone").HasColumnName("deleted_at");
            builder.HasKey(attempt => attempt.Id).HasName("pk_failed_login_attempts");
            builder.HasIndex(attempt => new { attempt.AccountId, attempt.AttemptedAt }).HasDatabaseName("ix_failed_login_attempts_account_id_attempted_at");
            builder.HasIndex(attempt => attempt.DeletedAt).HasDatabaseName("ix_failed_login_attempts_deleted_at");
            builder.HasIndex(attempt => new { attempt.Email, attempt.AttemptedAt }).HasDatabaseName("ix_failed_login_attempts_email_attempted_at");
            builder.HasQueryFilter(attempt => attempt.DeletedAt == null);
            builder.ToTable("failed_login_attempts");
        });

        modelBuilder.Entity<EmailVerificationToken>()
            .HasOne<Account>()
            .WithMany()
            .HasForeignKey(token => token.AccountId)
            .HasConstraintName("fk_email_verification_tokens_accounts_account_id")
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        modelBuilder.Entity<PasswordResetToken>()
            .HasOne<Account>()
            .WithMany()
            .HasForeignKey(token => token.AccountId)
            .HasConstraintName("fk_password_reset_tokens_accounts_account_id")
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        modelBuilder.Entity<RefreshToken>()
            .HasOne<Account>()
            .WithMany()
            .HasForeignKey(token => token.AccountId)
            .HasConstraintName("fk_refresh_tokens_accounts_account_id")
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        modelBuilder.Entity<FailedLoginAttempt>()
            .HasOne<Account>()
            .WithMany()
            .HasForeignKey(attempt => attempt.AccountId)
            .HasConstraintName("fk_failed_login_attempts_accounts_account_id")
            .OnDelete(DeleteBehavior.SetNull);
    }
}
