using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoCare.Application.Data.Auth.Migrations;

[DbContext(typeof(AuthDbContext))]
[Migration("202609070002_AddAuthSecurityEntities")]
public partial class AddAuthSecurityEntities : Migration
{
    private static readonly string[] AccountIdExpiresAt = ["account_id", "expires_at"];
    private static readonly string[] AccountIdAttemptedAt = ["account_id", "attempted_at"];
    private static readonly string[] EmailAttemptedAt = ["email", "attempted_at"];

    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "email_verification_tokens",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                account_id = table.Column<Guid>(type: "uuid", nullable: false),
                token = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                used_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                row_version = table.Column<Guid>(type: "uuid", nullable: false),
                deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_email_verification_tokens", x => x.id);
                table.ForeignKey("fk_email_verification_tokens_accounts_account_id", x => x.account_id,
                    principalTable: "accounts", principalColumn: "id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "failed_login_attempts",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                account_id = table.Column<Guid>(type: "uuid", nullable: true),
                email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                attempted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                ip_address = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                row_version = table.Column<Guid>(type: "uuid", nullable: false),
                deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_failed_login_attempts", x => x.id);
                table.ForeignKey("fk_failed_login_attempts_accounts_account_id", x => x.account_id,
                    principalTable: "accounts", principalColumn: "id", onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateTable(
            name: "password_reset_tokens",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                account_id = table.Column<Guid>(type: "uuid", nullable: false),
                token = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                used_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                row_version = table.Column<Guid>(type: "uuid", nullable: false),
                deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_password_reset_tokens", x => x.id);
                table.ForeignKey("fk_password_reset_tokens_accounts_account_id", x => x.account_id,
                    principalTable: "accounts", principalColumn: "id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "refresh_tokens",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                account_id = table.Column<Guid>(type: "uuid", nullable: false),
                token = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                revoked_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                user_agent = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                ip_address = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                row_version = table.Column<Guid>(type: "uuid", nullable: false),
                deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_refresh_tokens", x => x.id);
                table.ForeignKey("fk_refresh_tokens_accounts_account_id", x => x.account_id,
                    principalTable: "accounts", principalColumn: "id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex("ix_email_verification_tokens_account_id_expires_at", "email_verification_tokens", AccountIdExpiresAt);
        migrationBuilder.CreateIndex("ix_email_verification_tokens_deleted_at", "email_verification_tokens", "deleted_at");
        migrationBuilder.CreateIndex("ix_email_verification_tokens_token", "email_verification_tokens", "token", unique: true);
        migrationBuilder.CreateIndex("ix_failed_login_attempts_account_id_attempted_at", "failed_login_attempts", AccountIdAttemptedAt);
        migrationBuilder.CreateIndex("ix_failed_login_attempts_deleted_at", "failed_login_attempts", "deleted_at");
        migrationBuilder.CreateIndex("ix_failed_login_attempts_email_attempted_at", "failed_login_attempts", EmailAttemptedAt);
        migrationBuilder.CreateIndex("ix_password_reset_tokens_account_id_expires_at", "password_reset_tokens", AccountIdExpiresAt);
        migrationBuilder.CreateIndex("ix_password_reset_tokens_deleted_at", "password_reset_tokens", "deleted_at");
        migrationBuilder.CreateIndex("ix_password_reset_tokens_token", "password_reset_tokens", "token", unique: true);
        migrationBuilder.CreateIndex("ix_refresh_tokens_account_id_expires_at", "refresh_tokens", AccountIdExpiresAt);
        migrationBuilder.CreateIndex("ix_refresh_tokens_deleted_at", "refresh_tokens", "deleted_at");
        migrationBuilder.CreateIndex("ix_refresh_tokens_token", "refresh_tokens", "token", unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "email_verification_tokens");
        migrationBuilder.DropTable(name: "failed_login_attempts");
        migrationBuilder.DropTable(name: "password_reset_tokens");
        migrationBuilder.DropTable(name: "refresh_tokens");
    }
}
