using Location404.Data.Application.Common.Interfaces;
using Location404.Data.Domain.Entities;
using Location404.Data.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Location404.Data.Infrastructure.Repositories;

public class LocationRepository(GeoDataDbContext context) : ILocationRepository
{
    private readonly GeoDataDbContext _context = context;

    public async Task<Location?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Locations
            .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
    }

    public async Task<List<Location>> GetAllAsync(bool activeOnly = true, CancellationToken cancellationToken = default)
    {
        var query = _context.Locations.AsQueryable();

        if (activeOnly)
            query = query.Where(l => l.IsActive);

        return await query
            .OrderBy(l => l.Country)
            .ThenBy(l => l.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Location?> GetRandomAsync(CancellationToken cancellationToken = default)
    {
        var activeLocations = await _context.Locations
            .Where(l => l.IsActive)
            .ToListAsync(cancellationToken);

        if (activeLocations.Count == 0)
            return null;

        var random = new Random();
        var index = random.Next(activeLocations.Count);
        return activeLocations[index];
    }

    public async Task<List<Location>> GetByCountryAsync(string country, CancellationToken cancellationToken = default)
    {
        return await _context.Locations
            .Where(l => l.Country == country && l.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Location>> GetByRegionAsync(string region, CancellationToken cancellationToken = default)
    {
        return await _context.Locations
            .Where(l => l.Region == region && l.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Location>> GetByTagAsync(string tag, CancellationToken cancellationToken = default)
    {
        return await _context.Locations
            .Where(l => l.Tags.Contains(tag) && l.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<Location> AddAsync(Location location, CancellationToken cancellationToken = default)
    {
        await _context.Locations.AddAsync(location, cancellationToken);
        return location;
    }

    public Task UpdateAsync(Location location, CancellationToken cancellationToken = default)
    {
        _context.Locations.Update(location);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var location = await GetByIdAsync(id, cancellationToken);
        if (location != null)
        {
            _context.Locations.Remove(location);
        }
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Locations
            .AnyAsync(l => l.Id == id, cancellationToken);
    }
}
