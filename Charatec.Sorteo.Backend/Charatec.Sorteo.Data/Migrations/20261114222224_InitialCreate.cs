using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Charatec.Sorteo.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FechaRegistro = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlanesSuscripcion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    PrecioMensual = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DiasPruebaGratis = table.Column<int>(type: "integer", nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    FechaCreacion = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanesSuscripcion", x => x.Id);
                    table.CheckConstraint("CK_PlanSuscripcion_DiasPrueba", "\"DiasPruebaGratis\" >= 0");
                    table.CheckConstraint("CK_PlanSuscripcion_Precio", "\"PrecioMensual\" >= 0");
                });

            migrationBuilder.CreateTable(
                name: "Productos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(3000)", maxLength: 3000, nullable: false),
                    Marca = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Modelo = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    NumeroParte = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    ImagenUrl = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Costo = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
                    FechaCreacion = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Productos", x => x.Id);
                    table.CheckConstraint("CK_Producto_Costo", "\"Costo\" >= 0");
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Clientes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    FechaNacimiento = table.Column<DateOnly>(type: "date", nullable: true),
                    Nombres = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Apellidos = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    NumeroDocumento = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Telefono = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    FechaCreacion = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clientes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Clientes_AspNetUsers_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EventosAuditoria",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: true),
                    Entidad = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    EntidadId = table.Column<Guid>(type: "uuid", nullable: true),
                    Accion = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    DatosAnteriores = table.Column<string>(type: "text", nullable: true),
                    DatosNuevos = table.Column<string>(type: "text", nullable: true),
                    Fecha = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventosAuditoria", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventosAuditoria_AspNetUsers_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Sorteos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AdministradorUsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    Titulo = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: false),
                    ImagenPortadaUrl = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    AporteMinimo = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    MetaRecaudacion = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    FechaInicio = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    FechaCierre = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    FechaSorteo = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Estado = table.Column<int>(type: "integer", nullable: false),
                    FechaCreacion = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    FechaActualizacion = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sorteos", x => x.Id);
                    table.CheckConstraint("CK_Sorteo_AporteMinimo", "\"AporteMinimo\" > 0");
                    table.CheckConstraint("CK_Sorteo_Fechas", "\"FechaCierre\" > \"FechaInicio\"");
                    table.CheckConstraint("CK_Sorteo_MetaRecaudacion", "\"MetaRecaudacion\" IS NULL OR \"MetaRecaudacion\" > 0");
                    table.ForeignKey(
                        name: "FK_Sorteos_AspNetUsers_AdministradorUsuarioId",
                        column: x => x.AdministradorUsuarioId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Suscripciones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClienteId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlanSuscripcionId = table.Column<Guid>(type: "uuid", nullable: false),
                    FechaInicio = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    FechaFinPrueba = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    FechaProximoCobro = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    FechaFin = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    FechaCancelacion = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Estado = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Suscripciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Suscripciones_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Suscripciones_PlanesSuscripcion_PlanSuscripcionId",
                        column: x => x.PlanSuscripcionId,
                        principalTable: "PlanesSuscripcion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EjecucionesSorteo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SorteoId = table.Column<Guid>(type: "uuid", nullable: false),
                    FechaCreacion = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    FechaEjecucion = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    AlgoritmoVersion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    HashPadron = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Estado = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EjecucionesSorteo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EjecucionesSorteo_Sorteos_SorteoId",
                        column: x => x.SorteoId,
                        principalTable: "Sorteos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Participaciones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClienteId = table.Column<Guid>(type: "uuid", nullable: false),
                    SorteoId = table.Column<Guid>(type: "uuid", nullable: false),
                    FechaInscripcion = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Estado = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Participaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Participaciones_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Participaciones_Sorteos_SorteoId",
                        column: x => x.SorteoId,
                        principalTable: "Sorteos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PremiosSorteo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SorteoId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Cantidad = table.Column<int>(type: "integer", nullable: false),
                    OrdenPremio = table.Column<int>(type: "integer", nullable: false),
                    CostoAlPublicar = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PremiosSorteo", x => x.Id);
                    table.CheckConstraint("CK_PremioSorteo_Cantidad", "\"Cantidad\" > 0");
                    table.CheckConstraint("CK_PremioSorteo_Costo", "\"CostoAlPublicar\" >= 0");
                    table.ForeignKey(
                        name: "FK_PremiosSorteo_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PremiosSorteo_Sorteos_SorteoId",
                        column: x => x.SorteoId,
                        principalTable: "Sorteos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrdenesPago",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClienteId = table.Column<Guid>(type: "uuid", nullable: false),
                    SuscripcionId = table.Column<Guid>(type: "uuid", nullable: true),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    Monto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Moneda = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    IdempotencyKey = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Estado = table.Column<int>(type: "integer", nullable: false),
                    FechaCreacion = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    FechaActualizacion = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrdenesPago", x => x.Id);
                    table.CheckConstraint("CK_OrdenPago_Monto", "\"Monto\" > 0");
                    table.ForeignKey(
                        name: "FK_OrdenesPago_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrdenesPago_Suscripciones_SuscripcionId",
                        column: x => x.SuscripcionId,
                        principalTable: "Suscripciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DetallesPadronSorteo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EjecucionSorteoId = table.Column<Guid>(type: "uuid", nullable: false),
                    ParticipacionId = table.Column<Guid>(type: "uuid", nullable: false),
                    PesoParticipacion = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetallesPadronSorteo", x => x.Id);
                    table.CheckConstraint("CK_DetallePadron_Peso", "\"PesoParticipacion\" > 0");
                    table.ForeignKey(
                        name: "FK_DetallesPadronSorteo_EjecucionesSorteo_EjecucionSorteoId",
                        column: x => x.EjecucionSorteoId,
                        principalTable: "EjecucionesSorteo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DetallesPadronSorteo_Participaciones_ParticipacionId",
                        column: x => x.ParticipacionId,
                        principalTable: "Participaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ResultadosSorteo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EjecucionSorteoId = table.Column<Guid>(type: "uuid", nullable: false),
                    ParticipacionId = table.Column<Guid>(type: "uuid", nullable: false),
                    PremioSorteoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Posicion = table.Column<int>(type: "integer", nullable: false),
                    FechaSeleccion = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EstadoEntrega = table.Column<int>(type: "integer", nullable: false),
                    FechaEntrega = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResultadosSorteo", x => x.Id);
                    table.CheckConstraint("CK_Resultado_Posicion", "\"Posicion\" > 0");
                    table.ForeignKey(
                        name: "FK_ResultadosSorteo_EjecucionesSorteo_EjecucionSorteoId",
                        column: x => x.EjecucionSorteoId,
                        principalTable: "EjecucionesSorteo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ResultadosSorteo_Participaciones_ParticipacionId",
                        column: x => x.ParticipacionId,
                        principalTable: "Participaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ResultadosSorteo_PremiosSorteo_PremioSorteoId",
                        column: x => x.PremioSorteoId,
                        principalTable: "PremiosSorteo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Aportes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ParticipacionId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrdenPagoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Monto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Estado = table.Column<int>(type: "integer", nullable: false),
                    FechaCreacion = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    FechaConfirmacion = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Aportes", x => x.Id);
                    table.CheckConstraint("CK_Aporte_Monto", "\"Monto\" > 0");
                    table.ForeignKey(
                        name: "FK_Aportes_OrdenesPago_OrdenPagoId",
                        column: x => x.OrdenPagoId,
                        principalTable: "OrdenesPago",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Aportes_Participaciones_ParticipacionId",
                        column: x => x.ParticipacionId,
                        principalTable: "Participaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TransaccionesPago",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrdenPagoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Proveedor = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ReferenciaExterna = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    Estado = table.Column<int>(type: "integer", nullable: false),
                    FechaCreacion = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    FechaProcesamiento = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransaccionesPago", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransaccionesPago_OrdenesPago_OrdenPagoId",
                        column: x => x.OrdenPagoId,
                        principalTable: "OrdenesPago",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Aportes_OrdenPagoId",
                table: "Aportes",
                column: "OrdenPagoId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Aportes_ParticipacionId_Estado",
                table: "Aportes",
                columns: new[] { "ParticipacionId", "Estado" });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_AspNetUsers_NormalizedEmail",
                table: "AspNetUsers",
                column: "NormalizedEmail",
                unique: true,
                filter: "\"NormalizedEmail\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_UsuarioId",
                table: "Clientes",
                column: "UsuarioId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DetallesPadronSorteo_EjecucionSorteoId_ParticipacionId",
                table: "DetallesPadronSorteo",
                columns: new[] { "EjecucionSorteoId", "ParticipacionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DetallesPadronSorteo_ParticipacionId",
                table: "DetallesPadronSorteo",
                column: "ParticipacionId");

            migrationBuilder.CreateIndex(
                name: "IX_EjecucionesSorteo_SorteoId",
                table: "EjecucionesSorteo",
                column: "SorteoId");

            migrationBuilder.CreateIndex(
                name: "IX_EventosAuditoria_Entidad_EntidadId_Fecha",
                table: "EventosAuditoria",
                columns: new[] { "Entidad", "EntidadId", "Fecha" });

            migrationBuilder.CreateIndex(
                name: "IX_EventosAuditoria_UsuarioId",
                table: "EventosAuditoria",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesPago_ClienteId_IdempotencyKey",
                table: "OrdenesPago",
                columns: new[] { "ClienteId", "IdempotencyKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesPago_SuscripcionId",
                table: "OrdenesPago",
                column: "SuscripcionId");

            migrationBuilder.CreateIndex(
                name: "IX_Participaciones_ClienteId_SorteoId",
                table: "Participaciones",
                columns: new[] { "ClienteId", "SorteoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Participaciones_SorteoId",
                table: "Participaciones",
                column: "SorteoId");

            migrationBuilder.CreateIndex(
                name: "IX_PremiosSorteo_ProductoId",
                table: "PremiosSorteo",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_PremiosSorteo_SorteoId_OrdenPremio",
                table: "PremiosSorteo",
                columns: new[] { "SorteoId", "OrdenPremio" });

            migrationBuilder.CreateIndex(
                name: "IX_Productos_NumeroParte",
                table: "Productos",
                column: "NumeroParte");

            migrationBuilder.CreateIndex(
                name: "IX_ResultadosSorteo_EjecucionSorteoId_PremioSorteoId_Posicion",
                table: "ResultadosSorteo",
                columns: new[] { "EjecucionSorteoId", "PremioSorteoId", "Posicion" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ResultadosSorteo_ParticipacionId",
                table: "ResultadosSorteo",
                column: "ParticipacionId");

            migrationBuilder.CreateIndex(
                name: "IX_ResultadosSorteo_PremioSorteoId",
                table: "ResultadosSorteo",
                column: "PremioSorteoId");

            migrationBuilder.CreateIndex(
                name: "IX_Sorteos_AdministradorUsuarioId",
                table: "Sorteos",
                column: "AdministradorUsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Sorteos_Estado_FechaCierre",
                table: "Sorteos",
                columns: new[] { "Estado", "FechaCierre" });

            migrationBuilder.CreateIndex(
                name: "IX_Suscripciones_ClienteId_Estado",
                table: "Suscripciones",
                columns: new[] { "ClienteId", "Estado" });

            migrationBuilder.CreateIndex(
                name: "IX_Suscripciones_PlanSuscripcionId",
                table: "Suscripciones",
                column: "PlanSuscripcionId");

            migrationBuilder.CreateIndex(
                name: "IX_TransaccionesPago_OrdenPagoId",
                table: "TransaccionesPago",
                column: "OrdenPagoId");

            migrationBuilder.CreateIndex(
                name: "IX_TransaccionesPago_Proveedor_ReferenciaExterna",
                table: "TransaccionesPago",
                columns: new[] { "Proveedor", "ReferenciaExterna" },
                unique: true,
                filter: "\"ReferenciaExterna\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Aportes");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "DetallesPadronSorteo");

            migrationBuilder.DropTable(
                name: "EventosAuditoria");

            migrationBuilder.DropTable(
                name: "ResultadosSorteo");

            migrationBuilder.DropTable(
                name: "TransaccionesPago");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "EjecucionesSorteo");

            migrationBuilder.DropTable(
                name: "Participaciones");

            migrationBuilder.DropTable(
                name: "PremiosSorteo");

            migrationBuilder.DropTable(
                name: "OrdenesPago");

            migrationBuilder.DropTable(
                name: "Productos");

            migrationBuilder.DropTable(
                name: "Sorteos");

            migrationBuilder.DropTable(
                name: "Suscripciones");

            migrationBuilder.DropTable(
                name: "Clientes");

            migrationBuilder.DropTable(
                name: "PlanesSuscripcion");

            migrationBuilder.DropTable(
                name: "AspNetUsers");
        }
    }
}
