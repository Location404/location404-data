namespace Location404.Data.Application.DTOs;

/// <summary>
/// Coordinate data transfer object
/// Convention: X = Latitude, Y = Longitude
/// </summary>
public record CoordinateDto(double X, double Y)
{
    public double Latitude => X;
    public double Longitude => Y;
}
