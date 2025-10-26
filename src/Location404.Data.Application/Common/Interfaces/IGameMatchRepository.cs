using Location404.Data.Domain.Entities;

namespace Location404.Data.Application.Common.Interfaces;

public interface IGameMatchRepository
{
    Task<GameMatch?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<GameMatch>> GetByPlayerIdAsync(Guid playerId, int skip = 0, int take = 20, CancellationToken cancellationToken = default);
    Task<List<GameMatch>> GetRecentMatchesAsync(int count = 10, CancellationToken cancellationToken = default);
    Task<GameMatch> AddAsync(GameMatch match, CancellationToken cancellationToken = default);
    Task UpdateAsync(GameMatch match, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}
