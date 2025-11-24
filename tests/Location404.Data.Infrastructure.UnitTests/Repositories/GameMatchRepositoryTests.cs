using FluentAssertions;
using Location404.Data.Domain.Entities;
using Location404.Data.Domain.ValueObjects;
using Location404.Data.Infrastructure.Context;
using Location404.Data.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Location404.Data.Infrastructure.UnitTests.Repositories;

public class GameMatchRepositoryTests : IDisposable
{
    private readonly GeoDataDbContext _context;
    private readonly GameMatchRepository _sut;

    public GameMatchRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<GeoDataDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new GeoDataDbContext(options);
        _sut = new GameMatchRepository(_context);
    }

    [Fact]
    public async Task GetByIdAsync_WhenMatchExists_ShouldReturnMatch()
    {
        // Arrange
        var match = new GameMatch(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        await _context.Matches.AddAsync(match);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetByIdAsync(match.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(match.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WhenMatchDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _sut.GetByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByPlayerIdAsync_ShouldReturnPlayerMatches()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var match1 = new GameMatch(Guid.NewGuid(), playerId, Guid.NewGuid());
        var match2 = new GameMatch(Guid.NewGuid(), Guid.NewGuid(), playerId);
        var match3 = new GameMatch(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        await _context.Matches.AddRangeAsync(match1, match2, match3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetByPlayerIdAsync(playerId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(m => m.PlayerAId == playerId || m.PlayerBId == playerId);
    }

    [Fact]
    public async Task GetByPlayerIdAsync_WithPagination_ShouldRespectSkipAndTake()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var matches = new List<GameMatch>();
        for (int i = 0; i < 5; i++)
        {
            matches.Add(new GameMatch(Guid.NewGuid(), playerId, Guid.NewGuid()));
            await Task.Delay(10); // Ensure different timestamps
        }

        await _context.Matches.AddRangeAsync(matches);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetByPlayerIdAsync(playerId, skip: 2, take: 2);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetRecentMatchesAsync_ShouldReturnOnlyCompletedMatches()
    {
        // Arrange
        var match1 = new GameMatch(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        var match2 = new GameMatch(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        await _context.Matches.AddRangeAsync(match1, match2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetRecentMatchesAsync(10);

        // Assert
        result.Should().OnlyContain(m => m.IsCompleted);
    }

    [Fact]
    public async Task AddAsync_ShouldAddMatchToDatabase()
    {
        // Arrange
        var match = new GameMatch(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        // Act
        var result = await _sut.AddAsync(match);
        await _context.SaveChangesAsync();

        // Assert
        result.Should().Be(match);
        var saved = await _context.Matches.FindAsync(match.Id);
        saved.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateMatch()
    {
        // Arrange
        var match = new GameMatch(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        await _context.Matches.AddAsync(match);
        await _context.SaveChangesAsync();

        // Act
        await _sut.UpdateAsync(match);
        await _context.SaveChangesAsync();

        // Assert
        var updated = await _context.Matches.FindAsync(match.Id);
        updated.Should().NotBeNull();
    }

    [Fact]
    public async Task ExistsAsync_WhenMatchExists_ShouldReturnTrue()
    {
        // Arrange
        var match = new GameMatch(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        await _context.Matches.AddAsync(match);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.ExistsAsync(match.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WhenMatchDoesNotExist_ShouldReturnFalse()
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
