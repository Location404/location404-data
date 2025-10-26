using Location404.Data.Application.DTOs.Requests;
using Location404.Data.Application.DTOs.Responses;

namespace Location404.Data.Application.Common.Interfaces;

public interface ILocationService
{
    Task<LocationResponse?> GetLocationByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<LocationResponse>> GetAllLocationsAsync(bool activeOnly = true, CancellationToken cancellationToken = default);
    Task<LocationResponse?> GetRandomLocationAsync(CancellationToken cancellationToken = default);
    Task<LocationResponse> CreateLocationAsync(CreateLocationRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteLocationAsync(Guid id, CancellationToken cancellationToken = default);
}
