using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VideoCable.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AgregarDireccionInstalacionSuscripcion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DireccionInstalacion",
                table: "Suscripciones",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LocalidadInstalacion",
                table: "Suscripciones",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReferenciaInstalacion",
                table: "Suscripciones",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DireccionInstalacion",
                table: "Suscripciones");

            migrationBuilder.DropColumn(
                name: "LocalidadInstalacion",
                table: "Suscripciones");

            migrationBuilder.DropColumn(
                name: "ReferenciaInstalacion",
                table: "Suscripciones");
        }
    }
}
