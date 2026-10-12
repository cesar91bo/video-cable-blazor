using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VideoCable.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AgregarNumeroPrecintoSuscripcion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Suscripciones_CajaDistribucionId",
                table: "Suscripciones");

            migrationBuilder.AddColumn<string>(
                name: "NumeroPrecinto",
                table: "Suscripciones",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Suscripciones_CajaDistribucionId_NumeroPrecinto",
                table: "Suscripciones",
                columns: new[] { "CajaDistribucionId", "NumeroPrecinto" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Suscripciones_CajaDistribucionId_NumeroPrecinto",
                table: "Suscripciones");

            migrationBuilder.DropColumn(
                name: "NumeroPrecinto",
                table: "Suscripciones");

            migrationBuilder.CreateIndex(
                name: "IX_Suscripciones_CajaDistribucionId",
                table: "Suscripciones",
                column: "CajaDistribucionId");
        }
    }
}
