using FluentAssertions;
using Location404.Data.Application.Common.Interfaces;
using Location404.Data.Application.DTOs.Requests;
using Location404.Data.Application.Services;
using Location404.Data.Domain.Entities;
using Location404.Data.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Moq;

namespace Location404.Data.Application.UnitTests.Services;

public class LocationServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<LocationService>> _loggerMock;
    private readonly Mock<ILocationRepository> _locationRepositoryMock;
    private readonly LocationService _sut;

    public LocationServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<LocationService>>();
        _locationRepositoryMock = new Mock<ILocationRepository>();

        _unitOfWorkMock.Setup(x => x.Locations).Returns(_locationRepositoryMock.Object);

        _sut = new LocationService(_unitOfWorkMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetLocationByIdAsync_WhenLocationExists_ShouldReturnLocationResponse()
    {
        // Arrange
        var locationId = Guid.NewGuid();
        var coordinate = new Coordinate(-23.55, -46.63);
        var location = new Location(locationId, coordinate, "Avenida Paulista", "Brasil", "São Paulo");

        _locationRepositoryMock
            .Setup(x => x.GetByIdAsync(locationId, default))
            .ReturnsAsync(location);

        // Act
        var result = await _sut.GetLocationByIdAsync(locationId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(locationId);
        result.Name.Should().Be("Avenida Paulista");
        result.Country.Should().Be("Brasil");
        result.Coordinate.X.Should().Be(-23.55);
        result.Coordinate.Y.Should().Be(-46.63);
    }

    [Fact]
    public async Task GetLocationByIdAsync_WhenLocationDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var locationId = Guid.NewGuid();

        _locationRepositoryMock
            .Setup(x => x.GetByIdAsync(locationId, default))
            .ReturnsAsync((Location?)null);

        // Act
        var result = await _sut.GetLocationByIdAsync(locationId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllLocationsAsync_ShouldReturnAllActiveLocations()
    {
        // Arrange
        var locations = new List<Location>
        {
            new(Guid.NewGuid(), new Coordinate(-23.55, -46.63), "Location 1", "Brasil", "SP"),
            new(Guid.NewGuid(), new Coordinate(-22.90, -43.17), "Location 2", "Brasil", "RJ")
        };

        _locationRepositoryMock
            .Setup(x => x.GetAllAsync(true, default))
            .ReturnsAsync(locations);

        // Act
        var result = await _sut.GetAllLocationsAsync(activeOnly: true);

        // Assert
        result.Should().HaveCount(2);
        result[0].Name.Should().Be("Location 1");
        result[1].Name.Should().Be("Location 2");
    }

    [Fact]
    public async Task GetRandomLocationAsync_WhenLocationExists_ShouldReturnLocationResponse()
    {
        // Arrange
        var location = new Location(
            Guid.NewGuid(),
            new Coordinate(-23.55, -46.63),
            "Random Location",
            "Brasil",
            "SP"
        );

        _locationRepositoryMock
            .Setup(x => x.GetRandomAsync(default))
            .ReturnsAsync(location);

        // Act
        var result = await _sut.GetRandomLocationAsync();

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Random Location");
    }

    [Fact]
    public async Task GetRandomLocationAsync_WhenNoLocationExists_ShouldReturnNull()
    {
        // Arrange
        _locationRepositoryMock
            .Setup(x => x.GetRandomAsync(default))
            .ReturnsAsync((Location?)null);

        // Act
        var result = await _sut.GetRandomLocationAsync();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateLocationAsync_WithValidRequest_ShouldCreateLocationAndReturnResponse()
    {
        // Arrange
        var request = new CreateLocationRequest(
            Latitude: -23.55,
            Longitude: -46.63,
            Name: "Nova Localização",
            Country: "Brasil",
            Region: "São Paulo",
            Heading: 90,
            Pitch: 10,
            Tags: new List<string> { "urban", "historic" }
        );

        _locationRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Location>(), default))
            .ReturnsAsync((Location l, CancellationToken ct) => l);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(default))
            .ReturnsAsync(1);

        // Act
        var result = await _sut.CreateLocationAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Nova Localização");
        result.Country.Should().Be("Brasil");
        result.Coordinate.X.Should().Be(-23.55);
        result.Coordinate.Y.Should().Be(-46.63);
        result.Heading.Should().Be(90);
        result.Pitch.Should().Be(10);
        result.Tags.Should().Contain("urban");
        result.Tags.Should().Contain("historic");

        _locationRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Location>(), default), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task CreateLocationAsync_WithoutOptionalParameters_ShouldCreateLocationSuccessfully()
    {
        // Arrange
        var request = new CreateLocationRequest(
            Latitude: -23.55,
            Longitude: -46.63,
            Name: "Simple Location",
            Country: "Brasil",
            Region: "São Paulo",
            Heading: null,
            Pitch: null,
            Tags: null
        );

        _locationRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Location>(), default))
            .ReturnsAsync((Location l, CancellationToken ct) => l);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(default))
            .ReturnsAsync(1);

        // Act
        var result = await _sut.CreateLocationAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Simple Location");
        result.Heading.Should().BeNull();
        result.Pitch.Should().BeNull();
        result.Tags.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteLocationAsync_WhenLocationExists_ShouldDeactivateAndReturnTrue()
    {
        // Arrange
        var locationId = Guid.NewGuid();
        var location = new Location(
            locationId,
            new Coordinate(-23.55, -46.63),
            "Location to Delete",
            "Brasil",
            "SP"
        );

        _locationRepositoryMock
            .Setup(x => x.GetByIdAsync(locationId, default))
            .ReturnsAsync(location);

        _locationRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Location>(), default))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(default))
            .ReturnsAsync(1);

        // Act
        var result = await _sut.DeleteLocationAsync(locationId);

        // Assert
        result.Should().BeTrue();
        location.IsActive.Should().BeFalse();
        _locationRepositoryMock.Verify(x => x.UpdateAsync(location, default), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task DeleteLocationAsync_WhenLocationDoesNotExist_ShouldReturnFalse()
    {
        // Arrange
        var locationId = Guid.NewGuid();

        _locationRepositoryMock
            .Setup(x => x.GetByIdAsync(locationId, default))
            .ReturnsAsync((Location?)null);

        // Act
        var result = await _sut.DeleteLocationAsync(locationId);

        // Assert
        result.Should().BeFalse();
        _locationRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Location>(), default), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(default), Times.Never);
    }
}
