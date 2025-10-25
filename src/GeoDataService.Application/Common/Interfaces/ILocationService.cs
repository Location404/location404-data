using GeoDataService.Application.DTOs.Requests;
using GeoDataService.Application.DTOs.Responses;

namespace GeoDataService.Application.Common.Interfaces;

public interface ILocationService
{
    Task<LocationResponse?> GetLocationByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<LocationResponse>> GetAllLocationsAsync(bool activeOnly = true, CancellationToken cancellationToken = default);
    Task<LocationResponse?> GetRandomLocationAsync(CancellationToken cancellationToken = default);
    Task<LocationResponse> CreateLocationAsync(CreateLocationRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteLocationAsync(Guid id, CancellationToken cancellationToken = default);
}
