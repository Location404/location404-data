namespace Location404.Data.Application.DTOs.Requests;

/// <summary>
/// Request to create a new location
/// </summary>
public record CreateLocationRequest(
    double Latitude,
    double Longitude,
    string Name,
    string Country,
    string Region,
    int? Heading,
    int? Pitch,
    List<string>? Tags
);
