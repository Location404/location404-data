using GeoDataService.Application.DTOs.Responses;

namespace GeoDataService.Application.Common.Interfaces;

public interface IPlayerStatsService
{
    Task<PlayerStatsResponse?> GetPlayerStatsAsync(Guid playerId, CancellationToken cancellationToken = default);
    Task<List<PlayerStatsResponse>> GetRankingAsync(int count = 10, CancellationToken cancellationToken = default);
}
