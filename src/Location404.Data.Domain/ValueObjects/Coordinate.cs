namespace Location404.Data.Domain.ValueObjects;

/// <summary>
/// Value Object representing a geographic coordinate
/// Convention: X = Latitude, Y = Longitude
/// </summary>
public record Coordinate(double X, double Y)
{
    /// <summary>
    /// Latitude (North/South)
    /// </summary>
    public double Latitude => X;

    /// <summary>
    /// Longitude (East/West)
    /// </summary>
    public double Longitude => Y;

    /// <summary>
    /// Calculate distance between two coordinates using Haversine formula
    /// </summary>
    /// <param name="other">Target coordinate</param>
    /// <returns>Distance in kilometers</returns>
    public double CalculateDistanceInKm(Coordinate other)
    {
        const double earthRadiusKm = 6371.0;

        var lat1Rad = DegreesToRadians(Latitude);
        var lat2Rad = DegreesToRadians(other.Latitude);
        var deltaLat = DegreesToRadians(other.Latitude - Latitude);
        var deltaLon = DegreesToRadians(other.Longitude - Longitude);

        var a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        var distance = earthRadiusKm * c;

        return distance;
    }

    private static double DegreesToRadians(double degrees)
    {
        return degrees * Math.PI / 180.0;
    }
}
