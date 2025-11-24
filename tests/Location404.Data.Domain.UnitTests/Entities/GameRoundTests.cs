using FluentAssertions;
using Location404.Data.Domain.Entities;
using Location404.Data.Domain.ValueObjects;

namespace Location404.Data.Domain.UnitTests.Entities;

public class GameRoundTests
{
    private readonly Guid _matchId = Guid.NewGuid();
    private readonly Guid _locationId = Guid.NewGuid();
    private readonly Guid _playerAId = Guid.NewGuid();
    private readonly Guid _playerBId = Guid.NewGuid();
    private readonly Coordinate _correctAnswer = new(40.7128, -74.0060);

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateRound()
    {
        var roundId = Guid.NewGuid();

        var round = new GameRound(roundId, _matchId, 1, _locationId, _correctAnswer);

        round.Id.Should().Be(roundId);
        round.MatchId.Should().Be(_matchId);
        round.RoundNumber.Should().Be(1);
        round.LocationId.Should().Be(_locationId);
        round.CorrectAnswer.Should().Be(_correctAnswer);
        round.IsCompleted.Should().BeFalse();
        round.EndedAt.Should().BeNull();
        round.PlayerAGuess.Should().BeNull();
        round.PlayerBGuess.Should().BeNull();
    }

    [Fact]
    public void SetPlayers_ShouldSetPlayerIds()
    {
        var round = new GameRound(Guid.NewGuid(), _matchId, 1, _locationId, _correctAnswer);

        round.SetPlayers(_playerAId, _playerBId);

        round.PlayerAId.Should().Be(_playerAId);
        round.PlayerBId.Should().Be(_playerBId);
    }

    [Fact]
    public void SubmitGuess_ForPlayerA_ShouldCalculateDistanceAndPoints()
    {
        var round = new GameRound(Guid.NewGuid(), _matchId, 1, _locationId, _correctAnswer);
        round.SetPlayers(_playerAId, _playerBId);

        var guess = new Coordinate(40.7138, -74.0070);
        round.SubmitGuess(_playerAId, guess);

        round.PlayerAGuess.Should().Be(guess);
        round.PlayerADistance.Should().BeGreaterThan(0);
        round.PlayerAPoints.Should().BeGreaterThan(0);
        round.IsCompleted.Should().BeFalse();
    }

    [Fact]
    public void SubmitGuess_ForPlayerB_ShouldCalculateDistanceAndPoints()
    {
        var round = new GameRound(Guid.NewGuid(), _matchId, 1, _locationId, _correctAnswer);
        round.SetPlayers(_playerAId, _playerBId);

        var guess = new Coordinate(40.7138, -74.0070);
        round.SubmitGuess(_playerBId, guess);

        round.PlayerBGuess.Should().Be(guess);
        round.PlayerBDistance.Should().BeGreaterThan(0);
        round.PlayerBPoints.Should().BeGreaterThan(0);
        round.IsCompleted.Should().BeFalse();
    }

    [Fact]
    public void SubmitGuess_WithPerfectGuess_ShouldGive5000Points()
    {
        var round = new GameRound(Guid.NewGuid(), _matchId, 1, _locationId, _correctAnswer);
        round.SetPlayers(_playerAId, _playerBId);

        round.SubmitGuess(_playerAId, _correctAnswer);

        round.PlayerADistance.Should().Be(0);
        round.PlayerAPoints.Should().Be(5000);
    }

    [Fact]
    public void SubmitGuess_WithVeryFarGuess_ShouldGiveLowPoints()
    {
        var round = new GameRound(Guid.NewGuid(), _matchId, 1, _locationId, _correctAnswer);
        round.SetPlayers(_playerAId, _playerBId);

        var farGuess = new Coordinate(0, 0);
        round.SubmitGuess(_playerAId, farGuess);

        round.PlayerADistance.Should().BeGreaterThan(1000);
        round.PlayerAPoints.Should().BeLessThan(100);
    }

    [Fact]
    public void SubmitGuess_WhenBothPlayersGuess_ShouldCompleteRound()
    {
        var round = new GameRound(Guid.NewGuid(), _matchId, 1, _locationId, _correctAnswer);
        round.SetPlayers(_playerAId, _playerBId);

        round.SubmitGuess(_playerAId, _correctAnswer);
        round.SubmitGuess(_playerBId, _correctAnswer);

        round.IsCompleted.Should().BeTrue();
        round.EndedAt.Should().NotBeNull();
    }

    [Fact]
    public void SubmitGuess_WhenRoundIsCompleted_ShouldThrowInvalidOperationException()
    {
        var round = new GameRound(Guid.NewGuid(), _matchId, 1, _locationId, _correctAnswer);
        round.SetPlayers(_playerAId, _playerBId);

        round.SubmitGuess(_playerAId, _correctAnswer);
        round.SubmitGuess(_playerBId, _correctAnswer);

        var act = () => round.SubmitGuess(_playerAId, _correctAnswer);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Round is already completed");
    }

    [Fact]
    public void SubmitGuess_WithInvalidPlayerId_ShouldThrowArgumentException()
    {
        var round = new GameRound(Guid.NewGuid(), _matchId, 1, _locationId, _correctAnswer);
        round.SetPlayers(_playerAId, _playerBId);

        var invalidPlayerId = Guid.NewGuid();
        var act = () => round.SubmitGuess(invalidPlayerId, _correctAnswer);

        act.Should().Throw<ArgumentException>()
            .WithMessage("Player is not part of this round");
    }

    [Fact]
    public void SubmitGuess_MultipleTimes_ShouldUpdateGuess()
    {
        var round = new GameRound(Guid.NewGuid(), _matchId, 1, _locationId, _correctAnswer);
        round.SetPlayers(_playerAId, _playerBId);

        var firstGuess = new Coordinate(40.7, -74.0);
        var secondGuess = new Coordinate(40.71, -74.01);

        round.SubmitGuess(_playerAId, firstGuess);
        round.SubmitGuess(_playerAId, secondGuess);

        round.PlayerAGuess.Should().Be(secondGuess);
    }

    [Fact]
    public void CalculatePoints_WithZeroDistance_ShouldReturn5000()
    {
        var round = new GameRound(Guid.NewGuid(), _matchId, 1, _locationId, _correctAnswer);
        round.SetPlayers(_playerAId, _playerBId);

        round.SubmitGuess(_playerAId, _correctAnswer);

        round.PlayerAPoints.Should().Be(5000);
    }

    [Fact]
    public void CalculatePoints_WithFarDistance_ShouldReturnLowPoints()
    {
        var newYork = new Coordinate(40.7128, -74.0060);
        var saoPaulo = new Coordinate(-23.5505, -46.6333);

        var round = new GameRound(Guid.NewGuid(), _matchId, 1, _locationId, newYork);
        round.SetPlayers(_playerAId, _playerBId);

        round.SubmitGuess(_playerAId, saoPaulo);

        round.PlayerADistance.Should().BeGreaterThan(3000);
        round.PlayerAPoints.Should().BeLessThan(200);
    }

    [Fact]
    public void CalculatePoints_ShouldNeverReturnNegativePoints()
    {
        var round = new GameRound(Guid.NewGuid(), _matchId, 1, _locationId, _correctAnswer);
        round.SetPlayers(_playerAId, _playerBId);

        var antipodeGuess = new Coordinate(-40.7128, 105.994);
        round.SubmitGuess(_playerAId, antipodeGuess);

        round.PlayerAPoints.Should().BeGreaterThanOrEqualTo(0);
    }
}
