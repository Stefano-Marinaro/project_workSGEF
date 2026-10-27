using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoCare.Data.Migrations
{
    /// <inheritdoc />
    public partial class CoveredComuniInsteadOfProvinces : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_transport_requests_start_address_province",
                table: "transport_requests");

            migrationBuilder.AddUniqueConstraint(
                name: "ak_comuni_codice_istat",
                table: "comuni",
                column: "codice_istat");

            migrationBuilder.CreateTable(
                name: "association_covered_comuni",
                columns: table => new
                {
                    association_id = table.Column<Guid>(type: "uuid", nullable: false),
                    comune_codice_istat = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_association_covered_comuni", x => new { x.association_id, x.comune_codice_istat });
                    table.ForeignKey(
                        name: "fk_association_covered_comuni_associations_association_id",
                        column: x => x.association_id,
                        principalTable: "associations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_association_covered_comuni_comuni_comune_codice_istat",
                        column: x => x.comune_codice_istat,
                        principalTable: "comuni",
                        principalColumn: "codice_istat",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_association_covered_comuni_comune_codice_istat",
                table: "association_covered_comuni",
                column: "comune_codice_istat");

            migrationBuilder.Sql("""
                INSERT INTO association_covered_comuni (association_id, comune_codice_istat)
                SELECT a.id, c.codice_istat
                FROM associations a
                CROSS JOIN LATERAL unnest(a.covered_provinces) AS p(sigla)
                JOIN comuni c ON c.provincia = p.sigla
                ON CONFLICT DO NOTHING;
                """);

            migrationBuilder.DropColumn(
                name: "covered_provinces",
                table: "associations");
            // ogni provincia coperta diventa l'elenco di tutti i comuni di quella provincia

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<List<string>>(
                name: "covered_provinces",
                table: "associations",
                type: "text[]",
                nullable: true);

            // ricostruisce le province dai comuni coperti
            migrationBuilder.Sql("""
                UPDATE associations a
                SET covered_provinces = (
                    SELECT array_agg(DISTINCT c.provincia)
                    FROM association_covered_comuni acc
                    JOIN comuni c ON c.codice_istat = acc.comune_codice_istat
                    WHERE acc.association_id = a.id);
                """);
                
            migrationBuilder.DropTable(
                name: "association_covered_comuni");

            migrationBuilder.DropUniqueConstraint(
                name: "ak_comuni_codice_istat",
                table: "comuni");

            

            migrationBuilder.CreateIndex(
                name: "ix_transport_requests_start_address_province",
                table: "transport_requests",
                column: "start_address_province");
        }
    }
}
