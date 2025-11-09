using Location404.Data.Application.Common.Interfaces;
using Location404.Data.Application.DTOs.Responses;
using Microsoft.Extensions.Logging;

namespace Location404.Data.Application.Services;

public class PlayerStatsService(
    IUnitOfWork unitOfWork,
    ICacheService cacheService,
    ILogger<PlayerStatsService> logger) : IPlayerStatsService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ICacheService _cacheService = cacheService;
    private readonly ILogger<PlayerStatsService> _logger = logger;

    private const string RankingCacheKeyPrefix = "ranking:top";
    private const string PlayerStatsCacheKeyPrefix = "player:stats";
    private static readonly TimeSpan RankingCacheDuration = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan PlayerStatsCacheDuration = TimeSpan.FromMinutes(5);

    public async Task<PlayerStatsResponse?> GetPlayerStatsAsync(Guid playerId, CancellationToken cancellationToken = default)
    {
        // Cache-aside pattern: Try cache first
        var cacheKey = $"{PlayerStatsCacheKeyPrefix}:{playerId}";
        var cachedStats = await _cacheService.GetAsync<PlayerStatsResponse>(cacheKey, cancellationToken);

        if (cachedStats != null)
        {
            _logger.LogDebug("Cache hit for player stats: {PlayerId}", playerId);
            return cachedStats;
        }

        // Cache miss: Fetch from database
        var stats = await _unitOfWork.PlayerStats.GetByPlayerIdAsync(playerId, cancellationToken);

        if (stats == null)
            return null;

        var response = MapToResponse(stats);

        // Store in cache
        await _cacheService.SetAsync(cacheKey, response, PlayerStatsCacheDuration, cancellationToken);

        return response;
    }

    public async Task<List<PlayerStatsResponse>> GetRankingAsync(int count = 10, CancellationToken cancellationToken = default)
    {
        // Cache-aside pattern: Try cache first
        var cacheKey = $"{RankingCacheKeyPrefix}:{count}";
        var cachedRanking = await _cacheService.GetAsync<List<PlayerStatsResponse>>(cacheKey, cancellationToken);

        if (cachedRanking != null)
        {
            _logger.LogDebug("Cache hit for ranking (count: {Count})", count);
            return cachedRanking;
        }

        // Cache miss: Fetch from database
        var topPlayers = await _unitOfWork.PlayerStats.GetTopByRankingAsync(count, cancellationToken);
        var response = topPlayers.Select(MapToResponse).ToList();

        // Store in cache (short TTL for frequently changing data)
        await _cacheService.SetAsync(cacheKey, response, RankingCacheDuration, cancellationToken);

        return response;
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
