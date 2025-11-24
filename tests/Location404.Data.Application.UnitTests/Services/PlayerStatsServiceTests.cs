using FluentAssertions;
using Location404.Data.Application.Common.Interfaces;
using Location404.Data.Application.DTOs.Responses;
using Location404.Data.Application.Services;
using Location404.Data.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;

namespace Location404.Data.Application.UnitTests.Services;

public class PlayerStatsServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<ILogger<PlayerStatsService>> _loggerMock;
    private readonly Mock<IPlayerStatsRepository> _playerStatsRepositoryMock;
    private readonly PlayerStatsService _sut;

    public PlayerStatsServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _cacheServiceMock = new Mock<ICacheService>();
        _loggerMock = new Mock<ILogger<PlayerStatsService>>();
        _playerStatsRepositoryMock = new Mock<IPlayerStatsRepository>();

        _unitOfWorkMock.Setup(x => x.PlayerStats).Returns(_playerStatsRepositoryMock.Object);

        _sut = new PlayerStatsService(_unitOfWorkMock.Object, _cacheServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetPlayerStatsAsync_WhenCacheHit_ShouldReturnCachedData()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var cachedStats = new PlayerStatsResponse(
            playerId,
            10,
            5,
            4,
            1,
            50.0,
            30,
            15000,
            5000,
            500.0,
            100.0,
            1500,
            DateTime.UtcNow
        );

        _cacheServiceMock
            .Setup(x => x.GetAsync<PlayerStatsResponse>($"player:stats:{playerId}", default))
            .ReturnsAsync(cachedStats);

        // Act
        var result = await _sut.GetPlayerStatsAsync(playerId);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(cachedStats);
        _playerStatsRepositoryMock.Verify(x => x.GetByPlayerIdAsync(It.IsAny<Guid>(), default), Times.Never);
    }

    [Fact]
    public async Task GetPlayerStatsAsync_WhenCacheMiss_ShouldFetchFromDatabaseAndCache()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var stats = new PlayerStats(playerId);

        _cacheServiceMock
            .Setup(x => x.GetAsync<PlayerStatsResponse>($"player:stats:{playerId}", default))
            .ReturnsAsync((PlayerStatsResponse?)null);

        _playerStatsRepositoryMock
            .Setup(x => x.GetByPlayerIdAsync(playerId, default))
            .ReturnsAsync(stats);

        _cacheServiceMock
            .Setup(x => x.SetAsync(
                $"player:stats:{playerId}",
                It.IsAny<PlayerStatsResponse>(),
                It.IsAny<TimeSpan>(),
                default))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.GetPlayerStatsAsync(playerId);

        // Assert
        result.Should().NotBeNull();
        result!.PlayerId.Should().Be(playerId);
        _playerStatsRepositoryMock.Verify(x => x.GetByPlayerIdAsync(playerId, default), Times.Once);
        _cacheServiceMock.Verify(x => x.SetAsync(
            $"player:stats:{playerId}",
            It.IsAny<PlayerStatsResponse>(),
            It.IsAny<TimeSpan>(),
            default), Times.Once);
    }

    [Fact]
    public async Task GetPlayerStatsAsync_WhenPlayerDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var playerId = Guid.NewGuid();

        _cacheServiceMock
            .Setup(x => x.GetAsync<PlayerStatsResponse>($"player:stats:{playerId}", default))
            .ReturnsAsync((PlayerStatsResponse?)null);

        _playerStatsRepositoryMock
            .Setup(x => x.GetByPlayerIdAsync(playerId, default))
            .ReturnsAsync((PlayerStats?)null);

        // Act
        var result = await _sut.GetPlayerStatsAsync(playerId);

        // Assert
        result.Should().BeNull();
        _cacheServiceMock.Verify(x => x.SetAsync(
            It.IsAny<string>(),
            It.IsAny<PlayerStatsResponse>(),
            It.IsAny<TimeSpan>(),
            default), Times.Never);
    }

    [Fact]
    public async Task GetRankingAsync_WhenCacheHit_ShouldReturnCachedRanking()
    {
        // Arrange
        var count = 10;
        var cachedRanking = new List<PlayerStatsResponse>
        {
            new(Guid.NewGuid(), 10, 8, 2, 0, 80.0, 30, 25000, 5000, 833.0, 50.0, 2000, DateTime.UtcNow),
            new(Guid.NewGuid(), 8, 5, 3, 0, 62.5, 24, 18000, 4500, 750.0, 75.0, 1800, DateTime.UtcNow)
        };

        _cacheServiceMock
            .Setup(x => x.GetAsync<List<PlayerStatsResponse>>($"ranking:top:{count}", default))
            .ReturnsAsync(cachedRanking);

        // Act
        var result = await _sut.GetRankingAsync(count);

        // Assert
        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(cachedRanking);
        _playerStatsRepositoryMock.Verify(x => x.GetTopByRankingAsync(It.IsAny<int>(), default), Times.Never);
    }

    [Fact]
    public async Task GetRankingAsync_WhenCacheMiss_ShouldFetchFromDatabaseAndCache()
    {
        // Arrange
        var count = 10;
        var topPlayers = new List<PlayerStats>
        {
            new(Guid.NewGuid()),
            new(Guid.NewGuid()),
            new(Guid.NewGuid())
        };

        _cacheServiceMock
            .Setup(x => x.GetAsync<List<PlayerStatsResponse>>($"ranking:top:{count}", default))
            .ReturnsAsync((List<PlayerStatsResponse>?)null);

        _playerStatsRepositoryMock
            .Setup(x => x.GetTopByRankingAsync(count, default))
            .ReturnsAsync(topPlayers);

        _cacheServiceMock
            .Setup(x => x.SetAsync(
                $"ranking:top:{count}",
                It.IsAny<List<PlayerStatsResponse>>(),
                It.IsAny<TimeSpan>(),
                default))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.GetRankingAsync(count);

        // Assert
        result.Should().HaveCount(3);
        _playerStatsRepositoryMock.Verify(x => x.GetTopByRankingAsync(count, default), Times.Once);
        _cacheServiceMock.Verify(x => x.SetAsync(
            $"ranking:top:{count}",
            It.IsAny<List<PlayerStatsResponse>>(),
            It.IsAny<TimeSpan>(),
            default), Times.Once);
    }

    [Fact]
    public async Task GetRankingAsync_WithDefaultCount_ShouldUse10AsDefault()
    {
        // Arrange
        var topPlayers = new List<PlayerStats>();

        _cacheServiceMock
            .Setup(x => x.GetAsync<List<PlayerStatsResponse>>($"ranking:top:10", default))
            .ReturnsAsync((List<PlayerStatsResponse>?)null);

        _playerStatsRepositoryMock
            .Setup(x => x.GetTopByRankingAsync(10, default))
            .ReturnsAsync(topPlayers);

        _cacheServiceMock
            .Setup(x => x.SetAsync(
                It.IsAny<string>(),
                It.IsAny<List<PlayerStatsResponse>>(),
                It.IsAny<TimeSpan>(),
                default))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.GetRankingAsync();

        // Assert
        _playerStatsRepositoryMock.Verify(x => x.GetTopByRankingAsync(10, default), Times.Once);
    }

    [Fact]
    public async Task GetRankingAsync_WithCustomCount_ShouldUseProvidedCount()
    {
        // Arrange
        var count = 25;
        var topPlayers = new List<PlayerStats>();

        _cacheServiceMock
            .Setup(x => x.GetAsync<List<PlayerStatsResponse>>($"ranking:top:{count}", default))
            .ReturnsAsync((List<PlayerStatsResponse>?)null);

        _playerStatsRepositoryMock
            .Setup(x => x.GetTopByRankingAsync(count, default))
            .ReturnsAsync(topPlayers);

        _cacheServiceMock
            .Setup(x => x.SetAsync(
                It.IsAny<string>(),
                It.IsAny<List<PlayerStatsResponse>>(),
                It.IsAny<TimeSpan>(),
                default))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.GetRankingAsync(count);

        // Assert
        _playerStatsRepositoryMock.Verify(x => x.GetTopByRankingAsync(count, default), Times.Once);
    }
}
