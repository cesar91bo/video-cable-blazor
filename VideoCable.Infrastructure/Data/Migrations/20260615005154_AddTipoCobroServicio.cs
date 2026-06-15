using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VideoCable.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTipoCobroServicio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TipoCobro",
                table: "ServiciosPlan",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ServiciosPlan_TipoCobro",
                table: "ServiciosPlan",
                column: "TipoCobro");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ServiciosPlan_TipoCobro",
                table: "ServiciosPlan");

            migrationBuilder.DropColumn(
                name: "TipoCobro",
                table: "ServiciosPlan");
        }
    }
}
