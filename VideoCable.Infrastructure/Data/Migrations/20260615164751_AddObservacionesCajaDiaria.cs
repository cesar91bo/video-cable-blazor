using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VideoCable.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddObservacionesCajaDiaria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Observaciones",
                table: "CajasDiarias",
                newName: "ObservacionesCierre");

            migrationBuilder.AddColumn<string>(
                name: "ObservacionesApertura",
                table: "CajasDiarias",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ObservacionesApertura",
                table: "CajasDiarias");

            migrationBuilder.RenameColumn(
                name: "ObservacionesCierre",
                table: "CajasDiarias",
                newName: "Observaciones");
        }
    }
}
