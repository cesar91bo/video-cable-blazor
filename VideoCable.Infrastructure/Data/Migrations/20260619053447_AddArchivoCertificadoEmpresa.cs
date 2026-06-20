using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VideoCable.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddArchivoCertificadoEmpresa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Password",
                table: "CertificadosEmpresa",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RutaArchivo",
                table: "CertificadosEmpresa",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Password",
                table: "CertificadosEmpresa");

            migrationBuilder.DropColumn(
                name: "RutaArchivo",
                table: "CertificadosEmpresa");
        }
    }
}
