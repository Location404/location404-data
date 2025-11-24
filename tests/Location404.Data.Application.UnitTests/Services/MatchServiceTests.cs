using FluentAssertions;
using Location404.Data.Application.Common.Interfaces;
using Location404.Data.Application.DTOs;
using Location404.Data.Application.DTOs.Events;
using Location404.Data.Application.Services;
using Location404.Data.Domain.Entities;
using Location404.Data.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Moq;

namespace Location404.Data.Application.UnitTests.Services;

public class MatchServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<ILogger<MatchService>> _loggerMock;
    private readonly Mock<IGameMatchRepository> _matchRepositoryMock;
    private readonly Mock<IPlayerStatsRepository> _playerStatsRepositoryMock;
    private readonly MatchService _sut;

    public MatchServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _cacheServiceMock = new Mock<ICacheService>();
        _loggerMock = new Mock<ILogger<MatchService>>();
        _matchRepositoryMock = new Mock<IGameMatchRepository>();
        _playerStatsRepositoryMock = new Mock<IPlayerStatsRepository>();

        _unitOfWorkMock.Setup(x => x.Matches).Returns(_matchRepositoryMock.Object);
        _unitOfWorkMock.Setup(x => x.PlayerStats).Returns(_playerStatsRepositoryMock.Object);

        _sut = new MatchService(_unitOfWorkMock.Object, _cacheServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task ProcessMatchEndedEventAsync_WithValidEvent_ShouldCreateMatchAndUpdateStats()
    {
        // Arrange
        var matchId = Guid.NewGuid();
        var playerAId = Guid.NewGuid();
        var playerBId = Guid.NewGuid();
        var roundId = Guid.NewGuid();

        var eventDto = new GameMatchEndedEventDto(
            matchId,
            playerAId,
            playerBId,
            playerAId,
            playerBId,
            5000,
            4500,
            100,
            50,
            DateTime.UtcNow.AddMinutes(-5),
            DateTime.UtcNow,
            new List<GameRoundEventDto>
            {
                new(
                    roundId,
                    matchId,
                    1,
                    playerAId,
                    playerBId,
                    2500,
                    2300,
                    new CoordinateDto(-23.55, -46.63),
                    new CoordinateDto(-23.56, -46.64),
                    new CoordinateDto(-23.54, -46.62),
                    true
                )
            }
        );

        var playerAStats = new PlayerStats(playerAId);
        var playerBStats = new PlayerStats(playerBId);

        _matchRepositoryMock
            .Setup(x => x.GetByIdAsync(matchId, default))
            .ReturnsAsync((GameMatch?)null);

        _playerStatsRepositoryMock
            .Setup(x => x.GetByPlayerIdAsync(playerAId, default))
            .ReturnsAsync(playerAStats);

        _playerStatsRepositoryMock
            .Setup(x => x.GetByPlayerIdAsync(playerBId, default))
            .ReturnsAsync(playerBStats);

        _matchRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<GameMatch>(), default))
            .ReturnsAsync((GameMatch m, CancellationToken ct) => m);

        _playerStatsRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<PlayerStats>(), default))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(default))
            .ReturnsAsync(1);

        _cacheServiceMock
            .Setup(x => x.RemoveAsync(It.IsAny<string>(), default))
            .Returns(Task.CompletedTask);

        _cacheServiceMock
            .Setup(x => x.RemoveByPatternAsync(It.IsAny<string>(), default))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.ProcessMatchEndedEventAsync(eventDto);

        // Assert
        _matchRepositoryMock.Verify(x => x.AddAsync(It.IsAny<GameMatch>(), default), Times.Once);
        _playerStatsRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<PlayerStats>(), default), Times.Exactly(2));
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task ProcessMatchEndedEventAsync_WhenMatchAlreadyExists_ShouldSkipProcessing()
    {
        // Arrange
        var matchId = Guid.NewGuid();
        var playerAId = Guid.NewGuid();
        var playerBId = Guid.NewGuid();

        var eventDto = new GameMatchEndedEventDto(
            matchId,
            playerAId,
            playerBId,
            playerAId,
            playerBId,
            5000,
            4500,
            100,
            50,
            DateTime.UtcNow.AddMinutes(-5),
            DateTime.UtcNow,
            new List<GameRoundEventDto>()
        );

        var existingMatch = new GameMatch(matchId, playerAId, playerBId);

        _matchRepositoryMock
            .Setup(x => x.GetByIdAsync(matchId, default))
            .ReturnsAsync(existingMatch);

        // Act
        await _sut.ProcessMatchEndedEventAsync(eventDto);

        // Assert
        _matchRepositoryMock.Verify(x => x.AddAsync(It.IsAny<GameMatch>(), default), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(default), Times.Never);
    }

    [Fact]
    public async Task ProcessMatchEndedEventAsync_WhenPlayerStatsDoNotExist_ShouldCreateNewStats()
    {
        // Arrange
        var matchId = Guid.NewGuid();
        var playerAId = Guid.NewGuid();
        var playerBId = Guid.NewGuid();
        var roundId = Guid.NewGuid();

        var eventDto = new GameMatchEndedEventDto(
            matchId,
            playerAId,
            playerBId,
            playerAId,
            playerBId,
            5000,
            4500,
            100,
            50,
            DateTime.UtcNow.AddMinutes(-5),
            DateTime.UtcNow,
            new List<GameRoundEventDto>
            {
                new(
                    roundId,
                    matchId,
                    1,
                    playerAId,
                    playerBId,
                    2500,
                    2300,
                    new CoordinateDto(-23.55, -46.63),
                    new CoordinateDto(-23.56, -46.64),
                    new CoordinateDto(-23.54, -46.62),
                    true
                )
            }
        );

        _matchRepositoryMock
            .Setup(x => x.GetByIdAsync(matchId, default))
            .ReturnsAsync((GameMatch?)null);

        _playerStatsRepositoryMock
            .Setup(x => x.GetByPlayerIdAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync((PlayerStats?)null);

        _matchRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<GameMatch>(), default))
            .ReturnsAsync((GameMatch m, CancellationToken ct) => m);

        _playerStatsRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<PlayerStats>(), default))
            .ReturnsAsync((PlayerStats s, CancellationToken ct) => s);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(default))
            .ReturnsAsync(1);

        _cacheServiceMock
            .Setup(x => x.RemoveAsync(It.IsAny<string>(), default))
            .Returns(Task.CompletedTask);

        _cacheServiceMock
            .Setup(x => x.RemoveByPatternAsync(It.IsAny<string>(), default))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.ProcessMatchEndedEventAsync(eventDto);

        // Assert
        _playerStatsRepositoryMock.Verify(x => x.AddAsync(It.IsAny<PlayerStats>(), default), Times.Exactly(2));
    }

    [Fact]
    public async Task GetMatchByIdAsync_WhenMatchExists_ShouldReturnMatchResponse()
    {
        // Arrange
        var matchId = Guid.NewGuid();
        var playerAId = Guid.NewGuid();
        var playerBId = Guid.NewGuid();
        var match = new GameMatch(matchId, playerAId, playerBId);

        var round = new GameRound(
            Guid.NewGuid(),
            matchId,
            1,
            Guid.NewGuid(),
            new Coordinate(-23.55, -46.63)
        );
        round.SetPlayers(playerAId, playerBId);
        match.AddRound(round);

        _matchRepositoryMock
            .Setup(x => x.GetByIdAsync(matchId, default))
            .ReturnsAsync(match);

        // Act
        var result = await _sut.GetMatchByIdAsync(matchId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(matchId);
        result.PlayerAId.Should().Be(playerAId);
        result.PlayerBId.Should().Be(playerBId);
        result.Rounds.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetMatchByIdAsync_WhenMatchDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var matchId = Guid.NewGuid();

        _matchRepositoryMock
            .Setup(x => x.GetByIdAsync(matchId, default))
            .ReturnsAsync((GameMatch?)null);

        // Act
        var result = await _sut.GetMatchByIdAsync(matchId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetPlayerMatchesAsync_ShouldReturnPlayerMatches()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var matches = new List<GameMatch>
        {
            new(Guid.NewGuid(), playerId, Guid.NewGuid()),
            new(Guid.NewGuid(), Guid.NewGuid(), playerId)
        };

        _matchRepositoryMock
            .Setup(x => x.GetByPlayerIdAsync(playerId, 0, 20, default))
            .ReturnsAsync(matches);

        // Act
        var result = await _sut.GetPlayerMatchesAsync(playerId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(m => m.PlayerAId == playerId || m.PlayerBId == playerId);
    }

    [Fact]
    public async Task GetPlayerMatchesAsync_WithPagination_ShouldRespectSkipAndTake()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var matches = new List<GameMatch>
        {
            new(Guid.NewGuid(), playerId, Guid.NewGuid())
        };

        _matchRepositoryMock
            .Setup(x => x.GetByPlayerIdAsync(playerId, 10, 5, default))
            .ReturnsAsync(matches);

        // Act
        var result = await _sut.GetPlayerMatchesAsync(playerId, skip: 10, take: 5);

        // Assert
        result.Should().HaveCount(1);
        _matchRepositoryMock.Verify(x => x.GetByPlayerIdAsync(playerId, 10, 5, default), Times.Once);
    }
}
