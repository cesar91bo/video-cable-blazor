using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VideoCable.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AgregarLimiteFacturacionMensualArca : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "LimiteFacturacionMensualArca",
                table: "Empresas",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LimiteFacturacionMensualArca",
                table: "Empresas");
        }
    }
}
