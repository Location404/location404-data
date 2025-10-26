using Location404.Data.Domain.Entities;

namespace Location404.Data.Application.Common.Interfaces;

public interface IPlayerStatsRepository
{
    Task<PlayerStats?> GetByPlayerIdAsync(Guid playerId, CancellationToken cancellationToken = default);
    Task<List<PlayerStats>> GetTopByRankingAsync(int count = 10, CancellationToken cancellationToken = default);
    Task<PlayerStats> AddAsync(PlayerStats stats, CancellationToken cancellationToken = default);
    Task UpdateAsync(PlayerStats stats, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid playerId, CancellationToken cancellationToken = default);
}
