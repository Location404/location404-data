namespace Location404.Data.Application.Common.Interfaces;

public interface IUnitOfWork : IDisposable
{
    ILocationRepository Locations { get; }
    IGameMatchRepository Matches { get; }
    IPlayerStatsRepository PlayerStats { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
