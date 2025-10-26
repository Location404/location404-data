using Location404.Data.Application.DTOs.Responses;

namespace Location404.Data.Application.Common.Interfaces;

public interface IPlayerStatsService
{
    Task<PlayerStatsResponse?> GetPlayerStatsAsync(Guid playerId, CancellationToken cancellationToken = default);
    Task<List<PlayerStatsResponse>> GetRankingAsync(int count = 10, CancellationToken cancellationToken = default);
}
