
using Charatec.Sorteo.Data.Identity;
using Charatec.Sorteo.Entities.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Charatec.Sorteo.Data.Context;

public class SorteoDbContext
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public SorteoDbContext(
        DbContextOptions<SorteoDbContext> options)
        : base(options)
    {
    }

    // ============================================
    // TABLAS DEL SISTEMA
    // ============================================

    public DbSet<Cliente> Clientes => Set<Cliente>();

    public DbSet<PlanSuscripcion> PlanesSuscripcion
        => Set<PlanSuscripcion>();

    public DbSet<Suscripcion> Suscripciones
        => Set<Suscripcion>();

    public DbSet<Producto> Productos => Set<Producto>();

    public DbSet<TblSorteo> Sorteos => Set<TblSorteo>();

    public DbSet<PremioSorteo> PremiosSorteo
        => Set<PremioSorteo>();

    public DbSet<Participacion> Participaciones
        => Set<Participacion>();

    public DbSet<Aporte> Aportes => Set<Aporte>();

    public DbSet<OrdenPago> OrdenesPago
        => Set<OrdenPago>();

    public DbSet<TransaccionPago> TransaccionesPago
        => Set<TransaccionPago>();

    public DbSet<EjecucionSorteo> EjecucionesSorteo
        => Set<EjecucionSorteo>();

    public DbSet<DetallePadronSorteo> DetallesPadron
        => Set<DetallePadronSorteo>();

    public DbSet<ResultadoSorteo> ResultadosSorteo
        => Set<ResultadoSorteo>();

    public DbSet<EventoAuditoria> EventosAuditoria
        => Set<EventoAuditoria>();

    // ============================================
    // CONFIGURACIÓN DE ENTIDADES Y RELACIONES
    // ============================================

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ========================================
        // 1. USUARIOS - ASP.NET CORE IDENTITY
        // ========================================

        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(x => x.FechaRegistro)
                .IsRequired();

            entity.Property(x => x.Activo)
                .HasDefaultValue(true);

            entity.HasIndex(x => x.NormalizedEmail)
                .IsUnique()
                .HasDatabaseName(
                    "UX_AspNetUsers_NormalizedEmail")
                .HasFilter(
                    "\"NormalizedEmail\" IS NOT NULL");
        });

        // ========================================
        // 2. CLIENTES
        // ========================================

        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.ToTable("Clientes");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Nombres)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(x => x.Apellidos)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(x => x.NumeroDocumento)
                .HasMaxLength(20);

            entity.Property(x => x.Telefono)
                .HasMaxLength(20);

            entity.Property(x => x.Activo)
                .HasDefaultValue(true);

            entity.Property(x => x.FechaCreacion)
                .IsRequired();

            entity.HasIndex(x => x.UsuarioId)
                .IsUnique();

            entity.HasOne<ApplicationUser>()
                .WithOne()
                .HasForeignKey<Cliente>(
                    x => x.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ========================================
        // 3. PLANES DE SUSCRIPCIÓN
        // ========================================

        modelBuilder.Entity<PlanSuscripcion>(entity =>
        {
            entity.ToTable("PlanesSuscripcion", t =>
            {
                t.HasCheckConstraint(
                    "CK_PlanSuscripcion_Precio",
                    "\"PrecioMensual\" >= 0");

                t.HasCheckConstraint(
                    "CK_PlanSuscripcion_DiasPrueba",
                    "\"DiasPruebaGratis\" >= 0");
            });

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Nombre)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.Descripcion)
                .HasMaxLength(1000);

            entity.Property(x => x.PrecioMensual)
                .HasPrecision(18, 2)
                .IsRequired();

            entity.Property(x => x.DiasPruebaGratis)
                .IsRequired();

            entity.Property(x => x.Activo)
                .HasDefaultValue(true);
        });

        // ========================================
        // 4. SUSCRIPCIONES
        // ========================================

        modelBuilder.Entity<Suscripcion>(entity =>
        {
            entity.ToTable("Suscripciones");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Estado)
                .HasConversion<int>()
                .IsRequired();

            entity.Property(x => x.FechaInicio)
                .IsRequired();

            entity.HasIndex(x => new
            {
                x.ClienteId,
                x.Estado
            });

            entity.HasOne(x => x.Cliente)
                .WithMany(x => x.Suscripciones)
                .HasForeignKey(x => x.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.PlanSuscripcion)
                .WithMany(x => x.Suscripciones)
                .HasForeignKey(x => x.PlanSuscripcionId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ========================================
        // 5. PRODUCTOS
        // ========================================

        modelBuilder.Entity<Producto>(entity =>
        {
            entity.ToTable("Productos", t =>
            {
                t.HasCheckConstraint(
                    "CK_Producto_Costo",
                    "\"Costo\" >= 0");
            });

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Nombre)
                .HasMaxLength(250)
                .IsRequired();

            entity.Property(x => x.Descripcion)
                .HasMaxLength(3000);

            entity.Property(x => x.Marca)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.Modelo)
                .HasMaxLength(150);

            entity.Property(x => x.NumeroParte)
                .HasMaxLength(150);

            entity.Property(x => x.ImagenUrl)
                .HasMaxLength(2000);

            entity.Property(x => x.Costo)
                .HasPrecision(18, 2)
                .IsRequired();

            entity.HasIndex(x => x.NumeroParte);
        });

        // ========================================
        // 6. SORTEOS
        // ========================================

        modelBuilder.Entity<TblSorteo>(entity =>
        {
            entity.ToTable("Sorteos", t =>
            {
                t.HasCheckConstraint(
                    "CK_Sorteo_AporteMinimo",
                    "\"AporteMinimo\" > 0");

                t.HasCheckConstraint(
                    "CK_Sorteo_MetaRecaudacion",
                    "\"MetaRecaudacion\" IS NULL " +
                    "OR \"MetaRecaudacion\" > 0");

                t.HasCheckConstraint(
                    "CK_Sorteo_Fechas",
                    "\"FechaCierre\" > \"FechaInicio\"");
            });

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Titulo)
                .HasMaxLength(250)
                .IsRequired();

            entity.Property(x => x.Descripcion)
                .HasMaxLength(5000)
                .IsRequired();

            entity.Property(x => x.ImagenPortadaUrl)
                .HasMaxLength(2000);

            entity.Property(x => x.AporteMinimo)
                .HasPrecision(18, 2)
                .IsRequired();

            entity.Property(x => x.MetaRecaudacion)
                .HasPrecision(18, 2);

            entity.Property(x => x.Estado)
                .HasConversion<int>()
                .IsRequired();

            entity.HasIndex(x => new
            {
                x.Estado,
                x.FechaCierre
            });

            entity.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(
                    x => x.AdministradorUsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ========================================
        // 7. PREMIOS DE SORTEOS
        // ========================================

        modelBuilder.Entity<PremioSorteo>(entity =>
        {
            entity.ToTable("PremiosSorteo", t =>
            {
                t.HasCheckConstraint(
                    "CK_PremioSorteo_Cantidad",
                    "\"Cantidad\" > 0");

                t.HasCheckConstraint(
                    "CK_PremioSorteo_Costo",
                    "\"CostoAlPublicar\" >= 0");
            });

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Cantidad)
                .IsRequired();

            entity.Property(x => x.CostoAlPublicar)
                .HasPrecision(18, 2);

            entity.HasIndex(x => new
            {
                x.SorteoId,
                x.OrdenPremio
            });

            entity.HasOne(x => x.Sorteo)
                .WithMany(x => x.Premios)
                .HasForeignKey(x => x.SorteoId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Producto)
                .WithMany(x => x.PremiosSorteo)
                .HasForeignKey(x => x.ProductoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ========================================
        // 8. PARTICIPACIONES
        // ========================================

        modelBuilder.Entity<Participacion>(entity =>
        {
            entity.ToTable("Participaciones");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Estado)
                .HasConversion<int>()
                .IsRequired();

            entity.Property(x => x.FechaInscripcion)
                .IsRequired();

            // Un cliente solo puede tener una
            // inscripción por sorteo.

            entity.HasIndex(x => new
            {
                x.ClienteId,
                x.SorteoId
            })
            .IsUnique();

            entity.HasIndex(x => x.SorteoId);

            entity.HasOne(x => x.Cliente)
                .WithMany(x => x.Participaciones)
                .HasForeignKey(x => x.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Sorteo)
                .WithMany(x => x.Participaciones)
                .HasForeignKey(x => x.SorteoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ========================================
        // 9. ÓRDENES DE PAGO
        // ========================================

        modelBuilder.Entity<OrdenPago>(entity =>
        {
            entity.ToTable("OrdenesPago", t =>
            {
                t.HasCheckConstraint(
                    "CK_OrdenPago_Monto",
                    "\"Monto\" > 0");
            });

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Monto)
                .HasPrecision(18, 2)
                .IsRequired();

            entity.Property(x => x.Moneda)
                .HasMaxLength(3)
                .IsRequired();

            entity.Property(x => x.IdempotencyKey)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(x => x.Tipo)
                .HasConversion<int>()
                .IsRequired();

            entity.Property(x => x.Estado)
                .HasConversion<int>()
                .IsRequired();

            // Impide duplicar una orden utilizando
            // la misma clave para el mismo cliente.

            entity.HasIndex(x => new
            {
                x.ClienteId,
                x.IdempotencyKey
            })
            .IsUnique();

            entity.HasOne(x => x.Cliente)
                .WithMany(x => x.OrdenesPago)
                .HasForeignKey(x => x.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Suscripcion)
                .WithMany(x => x.OrdenesPago)
                .HasForeignKey(x => x.SuscripcionId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ========================================
        // 10. APORTES
        // ========================================

        modelBuilder.Entity<Aporte>(entity =>
        {
            entity.ToTable("Aportes", t =>
            {
                t.HasCheckConstraint(
                    "CK_Aporte_Monto",
                    "\"Monto\" > 0");
            });

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Monto)
                .HasPrecision(18, 2)
                .IsRequired();

            entity.Property(x => x.Estado)
                .HasConversion<int>()
                .IsRequired();

            // Una orden solo puede financiar
            // un aporte.

            entity.HasIndex(x => x.OrdenPagoId)
                .IsUnique();

            entity.HasIndex(x => new
            {
                x.ParticipacionId,
                x.Estado
            });

            entity.HasOne(x => x.Participacion)
                .WithMany(x => x.Aportes)
                .HasForeignKey(x => x.ParticipacionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.OrdenPago)
                .WithOne(x => x.Aporte)
                .HasForeignKey<Aporte>(
                    x => x.OrdenPagoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ========================================
        // 11. TRANSACCIONES DE PAGO
        // ========================================

        modelBuilder.Entity<TransaccionPago>(entity =>
        {
            entity.ToTable("TransaccionesPago");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Proveedor)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.ReferenciaExterna)
                .HasMaxLength(250);

            entity.Property(x => x.Estado)
                .HasConversion<int>()
                .IsRequired();

            // Una referencia externa no debe
            // repetirse dentro del mismo proveedor.

            entity.HasIndex(x => new
            {
                x.Proveedor,
                x.ReferenciaExterna
            })
            .IsUnique()
            .HasFilter(
                "\"ReferenciaExterna\" IS NOT NULL");

            entity.HasOne(x => x.OrdenPago)
                .WithMany(x => x.Transacciones)
                .HasForeignKey(x => x.OrdenPagoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ========================================
        // 12. EJECUCIONES DE SORTEO
        // ========================================

        modelBuilder.Entity<EjecucionSorteo>(entity =>
        {
            entity.ToTable("EjecucionesSorteo");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.AlgoritmoVersion)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.HashPadron)
                .HasMaxLength(128)
                .IsRequired();

            entity.Property(x => x.Estado)
                .HasConversion<int>()
                .IsRequired();

            entity.HasIndex(x => x.SorteoId);

            entity.HasOne(x => x.Sorteo)
                .WithMany(x => x.Ejecuciones)
                .HasForeignKey(x => x.SorteoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ========================================
        // 13. PADRÓN DEL SORTEO
        // ========================================

        modelBuilder.Entity<DetallePadronSorteo>(entity =>
        {
            entity.ToTable("DetallesPadronSorteo", t =>
            {
                t.HasCheckConstraint(
                    "CK_DetallePadron_Peso",
                    "\"PesoParticipacion\" > 0");
            });

            entity.HasKey(x => x.Id);

            entity.Property(x => x.PesoParticipacion)
                .IsRequired();

            entity.HasIndex(x => new
            {
                x.EjecucionSorteoId,
                x.ParticipacionId
            })
            .IsUnique();

            entity.HasOne(x => x.EjecucionSorteo)
                .WithMany(x => x.DetallesPadron)
                .HasForeignKey(x => x.EjecucionSorteoId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Participacion)
                .WithMany(x => x.DetallesPadron)
                .HasForeignKey(x => x.ParticipacionId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ========================================
        // 14. RESULTADOS DEL SORTEO
        // ========================================

        modelBuilder.Entity<ResultadoSorteo>(entity =>
        {
            entity.ToTable("ResultadosSorteo", t =>
            {
                t.HasCheckConstraint(
                    "CK_Resultado_Posicion",
                    "\"Posicion\" > 0");
            });

            entity.HasKey(x => x.Id);

            entity.Property(x => x.EstadoEntrega)
                .HasConversion<int>()
                .IsRequired();

            entity.HasIndex(x => new
            {
                x.EjecucionSorteoId,
                x.PremioSorteoId,
                x.Posicion
            })
            .IsUnique();

            entity.HasOne(x => x.EjecucionSorteo)
                .WithMany(x => x.Resultados)
                .HasForeignKey(x => x.EjecucionSorteoId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Participacion)
                .WithMany(x => x.Resultados)
                .HasForeignKey(x => x.ParticipacionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.PremioSorteo)
                .WithMany(x => x.Resultados)
                .HasForeignKey(x => x.PremioSorteoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ========================================
        // 15. AUDITORÍA
        // ========================================

        modelBuilder.Entity<EventoAuditoria>(entity =>
        {
            entity.ToTable("EventosAuditoria");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Entidad)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(x => x.Accion)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(x => x.DatosAnteriores)
                .HasColumnType("text");

            entity.Property(x => x.DatosNuevos)
                .HasColumnType("text");

            entity.Property(x => x.Fecha)
                .IsRequired();

            entity.HasIndex(x => new
            {
                x.Entidad,
                x.EntidadId,
                x.Fecha
            });

            entity.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(x => x.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
