using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoCare.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCaregiverInviteLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "caregiver_invite_links",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    assisted_person_id = table.Column<Guid>(type: "uuid", nullable: false),
                    invited_by_person_id = table.Column<Guid>(type: "uuid", nullable: false),
                    token = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    revoked_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_caregiver_invite_links", x => x.id);
                    table.ForeignKey(
                        name: "fk_caregiver_invite_links_assisted_people_assisted_person_id",
                        column: x => x.assisted_person_id,
                        principalTable: "assisted_people",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_caregiver_invite_links_persons_invited_by_person_id",
                        column: x => x.invited_by_person_id,
                        principalTable: "persons",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_caregiver_invite_links_assisted_person_id",
                table: "caregiver_invite_links",
                column: "assisted_person_id");

            migrationBuilder.CreateIndex(
                name: "ix_caregiver_invite_links_invited_by_person_id",
                table: "caregiver_invite_links",
                column: "invited_by_person_id");

            migrationBuilder.CreateIndex(
                name: "ix_caregiver_invite_links_token",
                table: "caregiver_invite_links",
                column: "token",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "caregiver_invite_links");
        }
    }
}
