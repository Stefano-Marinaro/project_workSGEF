using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoCare.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddNoteToAssistedPerson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "note",
                table: "assisted_people",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "note",
                table: "assisted_people");
        }
    }
}
