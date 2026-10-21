using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoCare.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPreferredContactAndAlternativeContacts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "preferred_contact_person_id",
                table: "transport_requests",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "alternative_contacts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    transport_request_id = table.Column<Guid>(type: "uuid", nullable: false),
                    phone = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    note = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_alternative_contacts", x => x.id);
                    table.ForeignKey(
                        name: "fk_alternative_contacts_transport_requests_transport_request_id",
                        column: x => x.transport_request_id,
                        principalTable: "transport_requests",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_transport_requests_preferred_contact_person_id",
                table: "transport_requests",
                column: "preferred_contact_person_id");

            migrationBuilder.CreateIndex(
                name: "ix_alternative_contacts_transport_request_id",
                table: "alternative_contacts",
                column: "transport_request_id");

            migrationBuilder.AddForeignKey(
                name: "fk_transport_requests_persons_preferred_contact_person_id",
                table: "transport_requests",
                column: "preferred_contact_person_id",
                principalTable: "persons",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_transport_requests_persons_preferred_contact_person_id",
                table: "transport_requests");

            migrationBuilder.DropTable(
                name: "alternative_contacts");

            migrationBuilder.DropIndex(
                name: "ix_transport_requests_preferred_contact_person_id",
                table: "transport_requests");

            migrationBuilder.DropColumn(
                name: "preferred_contact_person_id",
                table: "transport_requests");
        }
    }
}
