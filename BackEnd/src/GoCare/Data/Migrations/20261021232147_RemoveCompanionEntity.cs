using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoCare.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCompanionEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "companions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "companions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    phone = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    relationship = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    surname = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    transport_request_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_companions", x => x.id);
                    table.ForeignKey(
                        name: "fk_companions_transport_requests_transport_request_id",
                        column: x => x.transport_request_id,
                        principalTable: "transport_requests",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_companions_transport_request_id",
                table: "companions",
                column: "transport_request_id");
        }
    }
}
