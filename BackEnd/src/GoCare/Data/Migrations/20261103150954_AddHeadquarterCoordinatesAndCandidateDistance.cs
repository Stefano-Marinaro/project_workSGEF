using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoCare.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddHeadquarterCoordinatesAndCandidateDistance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "distance_to_reach_km",
                table: "transport_request_candidates",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "headquarter_latitude",
                table: "associations",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "headquarter_longitude",
                table: "associations",
                type: "double precision",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "distance_to_reach_km",
                table: "transport_request_candidates");

            migrationBuilder.DropColumn(
                name: "headquarter_latitude",
                table: "associations");

            migrationBuilder.DropColumn(
                name: "headquarter_longitude",
                table: "associations");
        }
    }
}
