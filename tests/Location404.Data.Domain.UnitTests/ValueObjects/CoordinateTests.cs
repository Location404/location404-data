using FluentAssertions;
using Location404.Data.Domain.ValueObjects;

namespace Location404.Data.Domain.UnitTests.ValueObjects;

public class CoordinateTests
{
    [Fact]
    public void Constructor_WithValidValues_ShouldCreateCoordinate()
    {
        var x = 40.7128;
        var y = -74.0060;

        var coordinate = new Coordinate(x, y);

        coordinate.X.Should().Be(x);
        coordinate.Y.Should().Be(y);
        coordinate.Latitude.Should().Be(x);
        coordinate.Longitude.Should().Be(y);
    }

    [Fact]
    public void CalculateDistanceInKm_WithSameCoordinate_ShouldReturnZero()
    {
        var coord = new Coordinate(40.7128, -74.0060);

        var distance = coord.CalculateDistanceInKm(coord);

        distance.Should().Be(0);
    }

    [Fact]
    public void CalculateDistanceInKm_WithDifferentCoordinates_ShouldReturnCorrectDistance()
    {
        var newYork = new Coordinate(40.7128, -74.0060);
        var london = new Coordinate(51.5074, -0.1278);

        var distance = newYork.CalculateDistanceInKm(london);

        distance.Should().BeApproximately(5570, 10);
    }

    [Fact]
    public void CalculateDistanceInKm_ShouldBeSymmetric()
    {
        var coord1 = new Coordinate(40.7128, -74.0060);
        var coord2 = new Coordinate(51.5074, -0.1278);

        var distance1 = coord1.CalculateDistanceInKm(coord2);
        var distance2 = coord2.CalculateDistanceInKm(coord1);

        distance1.Should().Be(distance2);
    }

    [Fact]
    public void CalculateDistanceInKm_WithNearbyCoordinates_ShouldReturnSmallDistance()
    {
        var coord1 = new Coordinate(40.7128, -74.0060);
        var coord2 = new Coordinate(40.7138, -74.0070);

        var distance = coord1.CalculateDistanceInKm(coord2);

        distance.Should().BeLessThan(1);
    }

    [Fact]
    public void Record_WithEqualValues_ShouldBeEqual()
    {
        var coord1 = new Coordinate(40.7128, -74.0060);
        var coord2 = new Coordinate(40.7128, -74.0060);

        coord1.Should().Be(coord2);
        (coord1 == coord2).Should().BeTrue();
    }

    [Fact]
    public void Record_WithDifferentValues_ShouldNotBeEqual()
    {
        var coord1 = new Coordinate(40.7128, -74.0060);
        var coord2 = new Coordinate(51.5074, -0.1278);

        coord1.Should().NotBe(coord2);
        (coord1 != coord2).Should().BeTrue();
    }

    [Fact]
    public void LatitudeLongitude_ShouldMapToXY()
    {
        var latitude = -23.5505;
        var longitude = -46.6333;

        var coordinate = new Coordinate(latitude, longitude);

        coordinate.Latitude.Should().Be(latitude);
        coordinate.Longitude.Should().Be(longitude);
        coordinate.X.Should().Be(latitude);
        coordinate.Y.Should().Be(longitude);
    }
}
