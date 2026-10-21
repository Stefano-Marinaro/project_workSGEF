using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoCare.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddComuneTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "comuni",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codice_istat = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    nome_comune = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    provincia = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    cap_codes = table.Column<List<string>>(type: "text[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_comuni", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_comuni_codice_istat",
                table: "comuni",
                column: "codice_istat",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_comuni_nome_comune",
                table: "comuni",
                column: "nome_comune");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "comuni");
        }
    }
}
