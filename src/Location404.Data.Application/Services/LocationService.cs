using Location404.Data.Application.Common.Interfaces;
using Location404.Data.Application.DTOs;
using Location404.Data.Application.DTOs.Requests;
using Location404.Data.Application.DTOs.Responses;
using Location404.Data.Domain.Entities;
using Location404.Data.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Location404.Data.Application.Services;

public class LocationService(IUnitOfWork unitOfWork, ILogger<LocationService> logger) : ILocationService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ILogger<LocationService> _logger = logger;

    public async Task<LocationResponse?> GetLocationByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var location = await _unitOfWork.Locations.GetByIdAsync(id, cancellationToken);
        return location == null ? null : MapToResponse(location);
    }

    public async Task<List<LocationResponse>> GetAllLocationsAsync(bool activeOnly = true, CancellationToken cancellationToken = default)
    {
        var locations = await _unitOfWork.Locations.GetAllAsync(activeOnly, cancellationToken);
        return locations.Select(MapToResponse).ToList();
    }

    public async Task<LocationResponse?> GetRandomLocationAsync(CancellationToken cancellationToken = default)
    {
        var location = await _unitOfWork.Locations.GetRandomAsync(cancellationToken);
        return location == null ? null : MapToResponse(location);
    }

    public async Task<LocationResponse> CreateLocationAsync(CreateLocationRequest request, CancellationToken cancellationToken = default)
    {
        var coordinate = new Coordinate(request.Latitude, request.Longitude);
        var location = new Location(
            Guid.NewGuid(),
            coordinate,
            request.Name,
            request.Country,
            request.Region
        );

        if (request.Heading.HasValue && request.Pitch.HasValue)
        {
            location.SetStreetViewParams(request.Heading.Value, request.Pitch.Value);
        }

        if (request.Tags != null)
        {
            foreach (var tag in request.Tags)
            {
                location.AddTag(tag);
            }
        }

        await _unitOfWork.Locations.AddAsync(location, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Location created: {LocationId} - {LocationName}", location.Id, location.Name);

        return MapToResponse(location);
    }

    public async Task<bool> DeleteLocationAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var location = await _unitOfWork.Locations.GetByIdAsync(id, cancellationToken);
        if (location == null)
            return false;

        location.Deactivate();
        await _unitOfWork.Locations.UpdateAsync(location, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Location deactivated: {LocationId}", id);

        return true;
    }

    private static LocationResponse MapToResponse(Location location)
    {
        return new LocationResponse(
            location.Id,
            new CoordinateDto(location.Coordinate.X, location.Coordinate.Y),
            location.Name,
            location.Country,
            location.Region,
            location.Heading,
            location.Pitch,
            location.TimesUsed,
            location.AveragePoints,
            location.DifficultyRating,
            location.Tags,
            location.IsActive
        );
    }
}
