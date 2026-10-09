using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoCare.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedAdminAccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                INSERT INTO accounts (id, email, password_hash, role, status, created_at, email_verified_at)
                VALUES (
                    '00000000-0000-0000-0000-000000000001',
                    'admin@gocare.it',
                    'AQAAAAIAAYagAAAAEPQ9R0qtyP357gb0ZLYFPLxvVSAeoDVJuScugprCfAgUumomy5+0Y8ylrfajd3QG4w==',
                    'Admin',
                    'Active',
                    now(),
                    now()
                );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM accounts WHERE id = '00000000-0000-0000-0000-000000000001';");
        }
    }
}
