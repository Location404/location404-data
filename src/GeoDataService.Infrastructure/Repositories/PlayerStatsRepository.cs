using GeoDataService.Application.Common.Interfaces;
using GeoDataService.Domain.Entities;
using GeoDataService.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace GeoDataService.Infrastructure.Repositories;

public class PlayerStatsRepository(GeoDataDbContext context) : IPlayerStatsRepository
{
    private readonly GeoDataDbContext _context = context;

    public async Task<PlayerStats?> GetByPlayerIdAsync(Guid playerId, CancellationToken cancellationToken = default)
    {
        return await _context.PlayerStats
            .FirstOrDefaultAsync(ps => ps.PlayerId == playerId, cancellationToken);
    }

    public async Task<List<PlayerStats>> GetTopByRankingAsync(int count = 10, CancellationToken cancellationToken = default)
    {
        return await _context.PlayerStats
            .OrderByDescending(ps => ps.RankingPoints)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<PlayerStats> AddAsync(PlayerStats stats, CancellationToken cancellationToken = default)
    {
        await _context.PlayerStats.AddAsync(stats, cancellationToken);
        return stats;
    }

    public Task UpdateAsync(PlayerStats stats, CancellationToken cancellationToken = default)
    {
        _context.PlayerStats.Update(stats);
        return Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(Guid playerId, CancellationToken cancellationToken = default)
    {
        return await _context.PlayerStats
            .AnyAsync(ps => ps.PlayerId == playerId, cancellationToken);
    }
}
