using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoCare.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTripAddressCoordinates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "end_address_latitude",
                table: "transport_requests",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "end_address_longitude",
                table: "transport_requests",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "return_end_address_latitude",
                table: "transport_requests",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "return_end_address_longitude",
                table: "transport_requests",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "start_address_latitude",
                table: "transport_requests",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "start_address_longitude",
                table: "transport_requests",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "end_address_latitude",
                table: "transport_requests");

            migrationBuilder.DropColumn(
                name: "end_address_longitude",
                table: "transport_requests");

            migrationBuilder.DropColumn(
                name: "return_end_address_latitude",
                table: "transport_requests");

            migrationBuilder.DropColumn(
                name: "return_end_address_longitude",
                table: "transport_requests");

            migrationBuilder.DropColumn(
                name: "start_address_latitude",
                table: "transport_requests");

            migrationBuilder.DropColumn(
                name: "start_address_longitude",
                table: "transport_requests");
        }
    }
}
