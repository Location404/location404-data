using GeoDataService.Domain.Entities;

namespace GeoDataService.Application.Common.Interfaces;

public interface ILocationRepository
{
    Task<Location?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Location>> GetAllAsync(bool activeOnly = true, CancellationToken cancellationToken = default);
    Task<Location?> GetRandomAsync(CancellationToken cancellationToken = default);
    Task<List<Location>> GetByCountryAsync(string country, CancellationToken cancellationToken = default);
    Task<List<Location>> GetByRegionAsync(string region, CancellationToken cancellationToken = default);
    Task<List<Location>> GetByTagAsync(string tag, CancellationToken cancellationToken = default);
    Task<Location> AddAsync(Location location, CancellationToken cancellationToken = default);
    Task UpdateAsync(Location location, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}
