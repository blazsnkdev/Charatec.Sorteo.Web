using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Charatec.Sorteo.Data.Migrations
{
    /// <inheritdoc />
    public partial class AgregarEvidenciaPagoYape : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EvidenciaPagoYape",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SolicitudId = table.Column<Guid>(type: "uuid", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    SorteoId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrdenPagoId = table.Column<Guid>(type: "uuid", nullable: false),
                    AporteId = table.Column<Guid>(type: "uuid", nullable: false),
                    NumeroWhatsapp = table.Column<string>(type: "character varying(9)", maxLength: 9, nullable: false),
                    Email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                    CodigoOperacionYape = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FechaRegistro = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Verificado = table.Column<bool>(type: "boolean", nullable: false),
                    FechaVerificacion = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    AdministradorRevisorId = table.Column<Guid>(type: "uuid", nullable: true),
                    Observacion = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EvidenciaPagoYape", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EvidenciaPagoYape_Aportes_AporteId",
                        column: x => x.AporteId,
                        principalTable: "Aportes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EvidenciaPagoYape_OrdenesPago_OrdenPagoId",
                        column: x => x.OrdenPagoId,
                        principalTable: "OrdenesPago",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EvidenciaPagoYape_AporteId",
                table: "EvidenciaPagoYape",
                column: "AporteId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EvidenciaPagoYape_CodigoOperacionYape",
                table: "EvidenciaPagoYape",
                column: "CodigoOperacionYape",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EvidenciaPagoYape_OrdenPagoId",
                table: "EvidenciaPagoYape",
                column: "OrdenPagoId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EvidenciaPagoYape_SolicitudId",
                table: "EvidenciaPagoYape",
                column: "SolicitudId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EvidenciaPagoYape");
        }
    }
}
