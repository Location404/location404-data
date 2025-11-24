using FluentAssertions;
using Location404.Data.Infrastructure.Context;
using Location404.Data.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace Location404.Data.Infrastructure.UnitTests.Data;

public class DataSeederTests : IDisposable
{
    private readonly GeoDataDbContext _context;
    private readonly Mock<ILogger<DataSeeder>> _loggerMock;
    private readonly DataSeeder _sut;

    public DataSeederTests()
    {
        var options = new DbContextOptionsBuilder<GeoDataDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new GeoDataDbContext(options);
        _loggerMock = new Mock<ILogger<DataSeeder>>();
        _sut = new DataSeeder(_context, _loggerMock.Object);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task SeedAsync_WhenDatabaseIsEmpty_ShouldSeedLocations()
    {
        // Arrange
        var initialCount = await _context.Locations.CountAsync();

        // Act
        await _sut.SeedAsync();

        // Assert
        initialCount.Should().Be(0);
        var finalCount = await _context.Locations.CountAsync();
        finalCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task SeedAsync_WhenDatabaseAlreadySeeded_ShouldNotSeedAgain()
    {
        // Arrange
        await _sut.SeedAsync();
        var countAfterFirstSeed = await _context.Locations.CountAsync();

        // Act
        await _sut.SeedAsync();

        // Assert
        var countAfterSecondSeed = await _context.Locations.CountAsync();
        countAfterSecondSeed.Should().Be(countAfterFirstSeed);
    }

    [Fact]
    public async Task SeedAsync_ShouldSeed96Locations()
    {
        // Act
        await _sut.SeedAsync();

        // Assert
        var count = await _context.Locations.CountAsync();
        count.Should().Be(96);
    }

    [Fact]
    public async Task SeedAsync_ShouldCreateLocationsWithCoordinates()
    {
        // Act
        await _sut.SeedAsync();

        // Assert
        var locations = await _context.Locations.ToListAsync();
        locations.Should().AllSatisfy(location =>
        {
            location.Coordinate.Should().NotBeNull();
            location.Coordinate.Latitude.Should().BeInRange(-90, 90);
            location.Coordinate.Longitude.Should().BeInRange(-180, 180);
        });
    }

    [Fact]
    public async Task SeedAsync_ShouldCreateLocationsWithValidNames()
    {
        // Act
        await _sut.SeedAsync();

        // Assert
        var locations = await _context.Locations.ToListAsync();
        locations.Should().AllSatisfy(location =>
        {
            location.Name.Should().NotBeNullOrEmpty();
            location.Country.Should().NotBeNullOrEmpty();
            location.Region.Should().NotBeNullOrEmpty();
        });
    }

    [Fact]
    public async Task SeedAsync_ShouldCreateLocationsWithStreetViewParams()
    {
        // Act
        await _sut.SeedAsync();

        // Assert
        var locations = await _context.Locations.ToListAsync();
        locations.Should().AllSatisfy(location =>
        {
            location.Heading.Should().BeInRange(0, 359);
            location.Pitch.Should().BeInRange(-10, 10);
        });
    }

    [Fact]
    public async Task SeedAsync_ShouldIncludeBrazilianLocations()
    {
        // Act
        await _sut.SeedAsync();

        // Assert
        var brazilLocations = await _context.Locations
            .Where(l => l.Country == "Brazil")
            .ToListAsync();

        brazilLocations.Should().NotBeEmpty();
        brazilLocations.Should().Contain(l => l.Name.Contains("São Paulo"));
        brazilLocations.Should().Contain(l => l.Name.Contains("Rio de Janeiro"));
    }

    [Fact]
    public async Task SeedAsync_ShouldIncludeLocationsFromMultipleContinents()
    {
        // Act
        await _sut.SeedAsync();

        // Assert
        var regions = await _context.Locations
            .Select(l => l.Region)
            .Distinct()
            .ToListAsync();

        regions.Should().Contain("South America");
        regions.Should().Contain("North America");
        regions.Should().Contain("Europe");
        regions.Should().Contain("Asia");
        regions.Should().Contain("Africa");
        regions.Should().Contain("Oceania");
    }

    [Fact]
    public async Task SeedAsync_ShouldCreateLocationsWithTags()
    {
        // Act
        await _sut.SeedAsync();

        // Assert
        var locationsWithTags = await _context.Locations
            .Where(l => l.Tags.Any())
            .ToListAsync();

        locationsWithTags.Should().NotBeEmpty();
    }

    [Fact]
    public async Task SeedAsync_ShouldLogInformationWhenDatabaseIsEmpty()
    {
        // Act
        await _sut.SeedAsync();

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Seeding database")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SeedAsync_ShouldLogInformationWhenDatabaseAlreadySeeded()
    {
        // Arrange
        await _sut.SeedAsync();
        _loggerMock.Reset();

        // Act
        await _sut.SeedAsync();

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("already seeded")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
