using GeoDataService.Domain.ValueObjects;

namespace GeoDataService.Domain.Entities;

/// <summary>
/// Represents a geographic location that can be used in the game
/// </summary>
public class Location
{
    // EF Core constructor
    private Location() { }

    public Location(Guid id, Coordinate coordinate, string name, string country, string region)
    {
        Id = id;
        Coordinate = coordinate;
        Name = name;
        Country = country;
        Region = region;
    }

    public Guid Id { get; private set; }
    public Coordinate Coordinate { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string Country { get; private set; } = null!;
    public string Region { get; private set; } = null!;

    /// <summary>
    /// Street View heading (camera direction, 0-360)
    /// </summary>
    public int? Heading { get; private set; }

    /// <summary>
    /// Street View pitch (camera angle, -90 to 90)
    /// </summary>
    public int? Pitch { get; private set; }

    /// <summary>
    /// Times this location has been used in matches
    /// </summary>
    public int TimesUsed { get; private set; } = 0;

    /// <summary>
    /// Average points scored on this location
    /// </summary>
    public double? AveragePoints { get; private set; }

    /// <summary>
    /// Difficulty rating (1-5)
    /// </summary>
    public int? DifficultyRating { get; private set; }

    /// <summary>
    /// Tags for categorization (e.g., "urban", "rural", "mountain", "beach")
    /// </summary>
    public List<string> Tags { get; private set; } = [];

    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; private set; }

    public void SetStreetViewParams(int heading, int pitch)
    {
        Heading = heading;
        Pitch = pitch;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateStats(int pointsScored)
    {
        TimesUsed++;
        AveragePoints = AveragePoints.HasValue
            ? (AveragePoints.Value * (TimesUsed - 1) + pointsScored) / TimesUsed
            : pointsScored;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetDifficulty(int rating)
    {
        if (rating < 1 || rating > 5)
            throw new ArgumentException("Difficulty rating must be between 1 and 5");

        DifficultyRating = rating;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddTag(string tag)
    {
        if (!Tags.Contains(tag))
        {
            Tags.Add(tag);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }
}
