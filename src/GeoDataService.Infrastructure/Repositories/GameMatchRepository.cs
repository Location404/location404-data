using GeoDataService.Application.Common.Interfaces;
using GeoDataService.Domain.Entities;
using GeoDataService.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace GeoDataService.Infrastructure.Repositories;

public class GameMatchRepository(GeoDataDbContext context) : IGameMatchRepository
{
    private readonly GeoDataDbContext _context = context;

    public async Task<GameMatch?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Matches
            .Include(m => m.Rounds)
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
    }

    public async Task<List<GameMatch>> GetByPlayerIdAsync(Guid playerId, int skip = 0, int take = 20, CancellationToken cancellationToken = default)
    {
        return await _context.Matches
            .Include(m => m.Rounds)
            .Where(m => m.PlayerAId == playerId || m.PlayerBId == playerId)
            .OrderByDescending(m => m.StartedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<GameMatch>> GetRecentMatchesAsync(int count = 10, CancellationToken cancellationToken = default)
    {
        return await _context.Matches
            .Include(m => m.Rounds)
            .Where(m => m.IsCompleted)
            .OrderByDescending(m => m.EndedAt)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<GameMatch> AddAsync(GameMatch match, CancellationToken cancellationToken = default)
    {
        await _context.Matches.AddAsync(match, cancellationToken);
        return match;
    }

    public Task UpdateAsync(GameMatch match, CancellationToken cancellationToken = default)
    {
        _context.Matches.Update(match);
        return Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Matches
            .AnyAsync(m => m.Id == id, cancellationToken);
    }
}
