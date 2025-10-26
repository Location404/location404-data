using Location404.Data.Application.Common.Interfaces;
using Location404.Data.Application.DTOs.Responses;
using Microsoft.Extensions.Logging;

namespace Location404.Data.Application.Services;

public class PlayerStatsService(IUnitOfWork unitOfWork, ILogger<PlayerStatsService> logger) : IPlayerStatsService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ILogger<PlayerStatsService> _logger = logger;

    public async Task<PlayerStatsResponse?> GetPlayerStatsAsync(Guid playerId, CancellationToken cancellationToken = default)
    {
        var stats = await _unitOfWork.PlayerStats.GetByPlayerIdAsync(playerId, cancellationToken);
        return stats == null ? null : MapToResponse(stats);
    }

    public async Task<List<PlayerStatsResponse>> GetRankingAsync(int count = 10, CancellationToken cancellationToken = default)
    {
        var topPlayers = await _unitOfWork.PlayerStats.GetTopByRankingAsync(count, cancellationToken);
        return topPlayers.Select(MapToResponse).ToList();
    }

    private static PlayerStatsResponse MapToResponse(Domain.Entities.PlayerStats stats)
    {
        return new PlayerStatsResponse(
            stats.PlayerId,
            stats.TotalMatches,
            stats.Wins,
            stats.Losses,
            stats.Draws,
            stats.GetWinRate(),
            stats.TotalRoundsPlayed,
            stats.TotalPoints,
            stats.HighestScore,
            stats.AveragePointsPerRound,
            stats.AverageDistanceErrorKm,
            stats.RankingPoints,
            stats.LastMatchAt
        );
    }
}
