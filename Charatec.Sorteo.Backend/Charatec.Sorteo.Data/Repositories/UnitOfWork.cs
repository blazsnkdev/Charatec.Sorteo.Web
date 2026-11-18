using Charatec.Sorteo.Common.Interfaces;
using Charatec.Sorteo.Data.Context;
using Charatec.Sorteo.Data.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace Charatec.Sorteo.Data.Repositories;

public sealed class UnitOfWork : IUnitOfWork, IDisposable
{
    private readonly SorteoDbContext _dbContext;

    private IDbContextTransaction? _transaction;

    private bool _disposed;

    public ISorteoRepository _sorteoRepository { get;}
    public IParticipacionYapeRepository _participacionesYape { get; }

    public UnitOfWork(
        SorteoDbContext dbContext,
        ISorteoRepository sorteoRepository,
        IParticipacionYapeRepository participacionYapeRepository 
        )
    {
        _participacionesYape = participacionYapeRepository;
        _dbContext = dbContext;
        _sorteoRepository = sorteoRepository;
    }

    // ============================================
    // 1. INICIAR TRANSACCIÓN
    // ============================================

    public async Task BeginTransactionAsync()
    {
        ObjectDisposedException.ThrowIf(
            _disposed, this);

        if (_transaction is not null)
        {
            throw new InvalidOperationException(
                "Ya existe una transacción activa.");
        }

        _transaction = await _dbContext
            .Database.BeginTransactionAsync();
    }

    // ============================================
    // 2. GUARDAR CAMBIOS
    // ============================================

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(
            _disposed, this);

        return await _dbContext.SaveChangesAsync(cancellationToken);
    }

    // ============================================
    // 3. CONFIRMAR TRANSACCIÓN
    // ============================================

    public async Task CommitTransactionAsync()
    {
        ObjectDisposedException.ThrowIf(
            _disposed, this);

        if (_transaction is null)
        {
            throw new InvalidOperationException(
                "No existe una transacción activa.");
        }

        // Guarda cambios pendientes antes del commit.
        await _dbContext.SaveChangesAsync();

        await _transaction.CommitAsync();

        await _transaction.DisposeAsync();

        _transaction = null;
    }

    // ============================================
    // 4. REVERTIR TRANSACCIÓN
    // ============================================

    public async Task RollBackAsync()
    {
        ObjectDisposedException.ThrowIf(
            _disposed, this);

        if (_transaction is null)
        {
            return;
        }

        try
        {
            await _transaction.RollbackAsync();
        }
        finally
        {
            await _transaction.DisposeAsync();

            _transaction = null;

            // Evita conservar en memoria entidades
            // que pertenecían a la transacción fallida.
            _dbContext.ChangeTracker.Clear();
        }
    }

    // ============================================
    // 5. LIBERAR RECURSOS
    // ============================================

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        // Si queda una transacción sin confirmar,
        // al disponerla se descartan sus cambios.
        _transaction?.Dispose();

        _transaction = null;

        // No liberamos _dbContext aquí:
        // su ciclo de vida lo administra ASP.NET Core DI.

        _disposed = true;

        GC.SuppressFinalize(this);
    }
}
