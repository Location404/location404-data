using FluentAssertions;
using Location404.Data.Domain.Entities;
using Location404.Data.Infrastructure.Context;
using Location404.Data.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Location404.Data.Infrastructure.UnitTests.Repositories;

public class PlayerStatsRepositoryTests : IDisposable
{
    private readonly GeoDataDbContext _context;
    private readonly PlayerStatsRepository _sut;

    public PlayerStatsRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<GeoDataDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new GeoDataDbContext(options);
        _sut = new PlayerStatsRepository(_context);
    }

    [Fact]
    public async Task GetByPlayerIdAsync_WhenStatsExist_ShouldReturnStats()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var stats = new PlayerStats(playerId);
        await _context.PlayerStats.AddAsync(stats);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetByPlayerIdAsync(playerId);

        // Assert
        result.Should().NotBeNull();
        result!.PlayerId.Should().Be(playerId);
    }

    [Fact]
    public async Task GetByPlayerIdAsync_WhenStatsDoNotExist_ShouldReturnNull()
    {
        // Arrange
        var playerId = Guid.NewGuid();

        // Act
        var result = await _sut.GetByPlayerIdAsync(playerId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetTopByRankingAsync_ShouldReturnTopPlayersByRankingPoints()
    {
        // Arrange
        var player1 = new PlayerStats(Guid.NewGuid());
        var player2 = new PlayerStats(Guid.NewGuid());
        var player3 = new PlayerStats(Guid.NewGuid());

        var match1 = new GameMatch(Guid.NewGuid(), player1.PlayerId, player2.PlayerId);
        player1.UpdateAfterMatch(match1, new List<GameRound>());

        await _context.PlayerStats.AddRangeAsync(player1, player2, player3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetTopByRankingAsync(2);

        // Assert
        result.Should().HaveCount(2);
        result[0].RankingPoints.Should().BeGreaterThanOrEqualTo(result[1].RankingPoints);
    }

    [Fact]
    public async Task AddAsync_ShouldAddStatsToDatabase()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var stats = new PlayerStats(playerId);

        // Act
        var result = await _sut.AddAsync(stats);
        await _context.SaveChangesAsync();

        // Assert
        result.Should().Be(stats);
        var saved = await _context.PlayerStats.FindAsync(playerId);
        saved.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateStats()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var stats = new PlayerStats(playerId);
        await _context.PlayerStats.AddAsync(stats);
        await _context.SaveChangesAsync();

        var match = new GameMatch(Guid.NewGuid(), playerId, Guid.NewGuid());
        stats.UpdateAfterMatch(match, new List<GameRound>());

        // Act
        await _sut.UpdateAsync(stats);
        await _context.SaveChangesAsync();

        // Assert
        var updated = await _context.PlayerStats.FindAsync(playerId);
        updated!.TotalMatches.Should().Be(1);
    }

    [Fact]
    public async Task ExistsAsync_WhenStatsExist_ShouldReturnTrue()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var stats = new PlayerStats(playerId);
        await _context.PlayerStats.AddAsync(stats);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.ExistsAsync(playerId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WhenStatsDoNotExist_ShouldReturnFalse()
    {
        // Arrange
        var playerId = Guid.NewGuid();

        // Act
        var result = await _sut.ExistsAsync(playerId);

        // Assert
        result.Should().BeFalse();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
