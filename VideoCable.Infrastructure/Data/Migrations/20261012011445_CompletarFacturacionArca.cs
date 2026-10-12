using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VideoCable.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class CompletarFacturacionArca : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Ambiente",
                table: "FacturasElectronicas",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "NumeroSolicitado",
                table: "FacturasElectronicas",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PuntoVentaSolicitadoId",
                table: "FacturasElectronicas",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TipoComprobanteSolicitado",
                table: "FacturasElectronicas",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AmbienteArca",
                table: "Facturas",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CondicionIvaReceptorArca",
                table: "Facturas",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DocNroReceptorArca",
                table: "Facturas",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DocTipoReceptorArca",
                table: "Facturas",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AlicuotaIvaArca",
                table: "Empresas",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 21m);

            migrationBuilder.AddColumn<int>(
                name: "AmbienteArca",
                table: "Empresas",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<byte[]>(
                name: "ArchivoPfxProtegido",
                table: "CertificadosEmpresa",
                type: "varbinary(max)",
                nullable: true);

            // El flujo anterior emitía únicamente en homologación. Conservar
            // ese ambiente en los comprobantes existentes, sin reutilizar CAE de prueba.
            migrationBuilder.Sql("""
                UPDATE f SET AmbienteArca = p.Ambiente
                FROM Facturas f INNER JOIN PuntosVenta p ON p.Id = f.PuntoVentaId;
                UPDATE Facturas SET AmbienteArca = 1 WHERE TipoComprobante <> 99 AND AmbienteArca IS NULL;
                UPDATE e SET Ambiente = f.AmbienteArca
                FROM FacturasElectronicas e INNER JOIN Facturas f ON f.Id = e.FacturaId;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_FacturasElectronicas_Ambiente_PuntoVentaSolicitadoId_TipoComprobanteSolicitado_NumeroSolicitado",
                table: "FacturasElectronicas",
                columns: new[] { "Ambiente", "PuntoVentaSolicitadoId", "TipoComprobanteSolicitado", "NumeroSolicitado" },
                unique: true,
                filter: "[NumeroSolicitado] IS NOT NULL AND [Resultado] <> 'R'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FacturasElectronicas_Ambiente_PuntoVentaSolicitadoId_TipoComprobanteSolicitado_NumeroSolicitado",
                table: "FacturasElectronicas");

            migrationBuilder.DropColumn(
                name: "Ambiente",
                table: "FacturasElectronicas");

            migrationBuilder.DropColumn(
                name: "NumeroSolicitado",
                table: "FacturasElectronicas");

            migrationBuilder.DropColumn(
                name: "PuntoVentaSolicitadoId",
                table: "FacturasElectronicas");

            migrationBuilder.DropColumn(
                name: "TipoComprobanteSolicitado",
                table: "FacturasElectronicas");

            migrationBuilder.DropColumn(
                name: "AmbienteArca",
                table: "Facturas");

            migrationBuilder.DropColumn(
                name: "CondicionIvaReceptorArca",
                table: "Facturas");

            migrationBuilder.DropColumn(
                name: "DocNroReceptorArca",
                table: "Facturas");

            migrationBuilder.DropColumn(
                name: "DocTipoReceptorArca",
                table: "Facturas");

            migrationBuilder.DropColumn(
                name: "AlicuotaIvaArca",
                table: "Empresas");

            migrationBuilder.DropColumn(
                name: "AmbienteArca",
                table: "Empresas");

            migrationBuilder.DropColumn(
                name: "ArchivoPfxProtegido",
                table: "CertificadosEmpresa");
        }
    }
}
