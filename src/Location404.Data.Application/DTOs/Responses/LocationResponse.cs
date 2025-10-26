namespace Location404.Data.Application.DTOs.Responses;

/// <summary>
/// Location response for API
/// </summary>
public record LocationResponse(
    Guid Id,
    CoordinateDto Coordinate,
    string Name,
    string Country,
    string Region,
    int? Heading,
    int? Pitch,
    int TimesUsed,
    double? AveragePoints,
    int? DifficultyRating,
    List<string> Tags,
    bool IsActive
);
