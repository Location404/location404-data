using FluentAssertions;
using Location404.Data.Domain.Entities;
using Location404.Data.Domain.ValueObjects;
using Location404.Data.Infrastructure.Context;
using Location404.Data.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Location404.Data.Infrastructure.UnitTests.Repositories;

public class LocationRepositoryTests : IDisposable
{
    private readonly GeoDataDbContext _context;
    private readonly LocationRepository _sut;

    public LocationRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<GeoDataDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new GeoDataDbContext(options);
        _sut = new LocationRepository(_context);
    }

    [Fact]
    public async Task GetByIdAsync_WhenLocationExists_ShouldReturnLocation()
    {
        // Arrange
        var location = new Location(
            Guid.NewGuid(),
            new Coordinate(-23.55, -46.63),
            "Test Location",
            "Brasil",
            "SP"
        );
        await _context.Locations.AddAsync(location);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetByIdAsync(location.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(location.Id);
        result.Name.Should().Be("Test Location");
    }

    [Fact]
    public async Task GetByIdAsync_WhenLocationDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _sut.GetByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_WithActiveOnly_ShouldReturnOnlyActiveLocations()
    {
        // Arrange
        var activeLocation = new Location(Guid.NewGuid(), new Coordinate(-23.55, -46.63), "Active", "Brasil", "SP");
        var inactiveLocation = new Location(Guid.NewGuid(), new Coordinate(-22.90, -43.17), "Inactive", "Brasil", "RJ");
        inactiveLocation.Deactivate();

        await _context.Locations.AddRangeAsync(activeLocation, inactiveLocation);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetAllAsync(activeOnly: true);

        // Assert
        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Active");
    }

    [Fact]
    public async Task GetAllAsync_WithActiveOnlyFalse_ShouldReturnAllLocations()
    {
        // Arrange
        var activeLocation = new Location(Guid.NewGuid(), new Coordinate(-23.55, -46.63), "Active", "Brasil", "SP");
        var inactiveLocation = new Location(Guid.NewGuid(), new Coordinate(-22.90, -43.17), "Inactive", "Brasil", "RJ");
        inactiveLocation.Deactivate();

        await _context.Locations.AddRangeAsync(activeLocation, inactiveLocation);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetAllAsync(activeOnly: false);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetRandomAsync_WhenLocationsExist_ShouldReturnRandomLocation()
    {
        // Arrange
        var location1 = new Location(Guid.NewGuid(), new Coordinate(-23.55, -46.63), "Location 1", "Brasil", "SP");
        var location2 = new Location(Guid.NewGuid(), new Coordinate(-22.90, -43.17), "Location 2", "Brasil", "RJ");
        await _context.Locations.AddRangeAsync(location1, location2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetRandomAsync();

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Match(n => n == "Location 1" || n == "Location 2");
    }

    [Fact]
    public async Task GetRandomAsync_WhenNoActiveLocations_ShouldReturnNull()
    {
        // Arrange
        // No active locations in database

        // Act
        var result = await _sut.GetRandomAsync();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByCountryAsync_ShouldReturnLocationsFromCountry()
    {
        // Arrange
        var brazilLocation = new Location(Guid.NewGuid(), new Coordinate(-23.55, -46.63), "Brazil Loc", "Brasil", "SP");
        var usaLocation = new Location(Guid.NewGuid(), new Coordinate(40.71, -74.00), "USA Loc", "USA", "NY");
        await _context.Locations.AddRangeAsync(brazilLocation, usaLocation);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetByCountryAsync("Brasil");

        // Assert
        result.Should().HaveCount(1);
        result[0].Country.Should().Be("Brasil");
    }

    [Fact]
    public async Task GetByRegionAsync_ShouldReturnLocationsFromRegion()
    {
        // Arrange
        var spLocation = new Location(Guid.NewGuid(), new Coordinate(-23.55, -46.63), "SP Loc", "Brasil", "SP");
        var rjLocation = new Location(Guid.NewGuid(), new Coordinate(-22.90, -43.17), "RJ Loc", "Brasil", "RJ");
        await _context.Locations.AddRangeAsync(spLocation, rjLocation);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetByRegionAsync("SP");

        // Assert
        result.Should().HaveCount(1);
        result[0].Region.Should().Be("SP");
    }

    [Fact]
    public async Task GetByTagAsync_ShouldReturnLocationsWithTag()
    {
        // Arrange
        var urbanLocation = new Location(Guid.NewGuid(), new Coordinate(-23.55, -46.63), "Urban Loc", "Brasil", "SP");
        urbanLocation.AddTag("urban");

        var ruralLocation = new Location(Guid.NewGuid(), new Coordinate(-22.90, -43.17), "Rural Loc", "Brasil", "RJ");
        ruralLocation.AddTag("rural");

        await _context.Locations.AddRangeAsync(urbanLocation, ruralLocation);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetByTagAsync("urban");

        // Assert
        result.Should().HaveCount(1);
        result[0].Tags.Should().Contain("urban");
    }

    [Fact]
    public async Task AddAsync_ShouldAddLocationToDatabase()
    {
        // Arrange
        var location = new Location(
            Guid.NewGuid(),
            new Coordinate(-23.55, -46.63),
            "New Location",
            "Brasil",
            "SP"
        );

        // Act
        var result = await _sut.AddAsync(location);
        await _context.SaveChangesAsync();

        // Assert
        result.Should().Be(location);
        var savedLocation = await _context.Locations.FindAsync(location.Id);
        savedLocation.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateLocation()
    {
        // Arrange
        var location = new Location(Guid.NewGuid(), new Coordinate(-23.55, -46.63), "Original", "Brasil", "SP");
        await _context.Locations.AddAsync(location);
        await _context.SaveChangesAsync();

        location.Deactivate();

        // Act
        await _sut.UpdateAsync(location);
        await _context.SaveChangesAsync();

        // Assert
        var updated = await _context.Locations.FindAsync(location.Id);
        updated!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_WhenLocationExists_ShouldRemoveLocation()
    {
        // Arrange
        var location = new Location(Guid.NewGuid(), new Coordinate(-23.55, -46.63), "To Delete", "Brasil", "SP");
        await _context.Locations.AddAsync(location);
        await _context.SaveChangesAsync();

        // Act
        await _sut.DeleteAsync(location.Id);
        await _context.SaveChangesAsync();

        // Assert
        var deleted = await _context.Locations.FindAsync(location.Id);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_WhenLocationDoesNotExist_ShouldDoNothing()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        await _sut.DeleteAsync(nonExistentId);
        await _context.SaveChangesAsync();

        // Assert - should not throw
        var count = await _context.Locations.CountAsync();
        count.Should().Be(0);
    }

    [Fact]
    public async Task ExistsAsync_WhenLocationExists_ShouldReturnTrue()
    {
        // Arrange
        var location = new Location(Guid.NewGuid(), new Coordinate(-23.55, -46.63), "Exists", "Brasil", "SP");
        await _context.Locations.AddAsync(location);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.ExistsAsync(location.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WhenLocationDoesNotExist_ShouldReturnFalse()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _sut.ExistsAsync(nonExistentId);

        // Assert
        result.Should().BeFalse();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
