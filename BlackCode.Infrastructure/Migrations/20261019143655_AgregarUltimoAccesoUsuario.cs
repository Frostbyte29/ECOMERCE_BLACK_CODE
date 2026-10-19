using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlackCode.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarUltimoAccesoUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ultimo_acceso_utc",
                table: "usuarios",
                type: "datetime(6)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ultimo_acceso_utc",
                table: "usuarios");
        }
    }
}
