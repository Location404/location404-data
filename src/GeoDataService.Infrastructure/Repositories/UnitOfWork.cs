using GeoDataService.Application.Common.Interfaces;
using GeoDataService.Infrastructure.Context;
using Microsoft.EntityFrameworkCore.Storage;

namespace GeoDataService.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly GeoDataDbContext _context;
    private IDbContextTransaction? _transaction;

    public ILocationRepository Locations { get; }
    public IGameMatchRepository Matches { get; }
    public IPlayerStatsRepository PlayerStats { get; }

    public UnitOfWork(
        GeoDataDbContext context,
        ILocationRepository locationRepository,
        IGameMatchRepository matchRepository,
        IPlayerStatsRepository playerStatsRepository)
    {
        _context = context;
        Locations = locationRepository;
        Matches = matchRepository;
        PlayerStats = playerStatsRepository;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
