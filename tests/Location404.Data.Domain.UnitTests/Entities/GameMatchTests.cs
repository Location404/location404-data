using FluentAssertions;
using Location404.Data.Domain.Entities;
using Location404.Data.Domain.ValueObjects;

namespace Location404.Data.Domain.UnitTests.Entities;

public class GameMatchTests
{
    private readonly Guid _playerAId = Guid.NewGuid();
    private readonly Guid _playerBId = Guid.NewGuid();
    private readonly Coordinate _testCoordinate = new(40.7128, -74.0060);

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateMatch()
    {
        var matchId = Guid.NewGuid();

        var match = new GameMatch(matchId, _playerAId, _playerBId);

        match.Id.Should().Be(matchId);
        match.PlayerAId.Should().Be(_playerAId);
        match.PlayerBId.Should().Be(_playerBId);
        match.PlayerATotalPoints.Should().Be(0);
        match.PlayerBTotalPoints.Should().Be(0);
        match.WinnerId.Should().BeNull();
        match.LoserId.Should().BeNull();
        match.Rounds.Should().BeEmpty();
        match.IsCompleted.Should().BeFalse();
        match.EndedAt.Should().BeNull();
    }

    [Fact]
    public void AddRound_WithValidRound_ShouldAddToList()
    {
        var match = new GameMatch(Guid.NewGuid(), _playerAId, _playerBId);
        var round = new GameRound(Guid.NewGuid(), match.Id, 1, Guid.NewGuid(), _testCoordinate);

        match.AddRound(round);

        match.Rounds.Should().HaveCount(1);
        match.Rounds.Should().Contain(round);
    }

    [Fact]
    public void AddRound_WhenMatchHas3Rounds_ShouldThrowInvalidOperationException()
    {
        var match = new GameMatch(Guid.NewGuid(), _playerAId, _playerBId);

        for (int i = 1; i <= 3; i++)
        {
            var round = new GameRound(Guid.NewGuid(), match.Id, i, Guid.NewGuid(), _testCoordinate);
            match.AddRound(round);
        }

        var extraRound = new GameRound(Guid.NewGuid(), match.Id, 4, Guid.NewGuid(), _testCoordinate);
        var act = () => match.AddRound(extraRound);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("A match can only have 3 rounds");
    }

    [Fact]
    public void AddRound_WhenMatchIsCompleted_ShouldThrowInvalidOperationException()
    {
        var match = new GameMatch(Guid.NewGuid(), _playerAId, _playerBId);

        for (int i = 1; i <= 3; i++)
        {
            var round = new GameRound(Guid.NewGuid(), match.Id, i, Guid.NewGuid(), _testCoordinate);
            round.SetPlayers(_playerAId, _playerBId);
            match.AddRound(round);

            round.SubmitGuess(_playerAId, _testCoordinate);
            round.SubmitGuess(_playerBId, _testCoordinate);
            match.UpdateScores(round);
        }

        var newRound = new GameRound(Guid.NewGuid(), match.Id, 4, Guid.NewGuid(), _testCoordinate);
        var act = () => match.AddRound(newRound);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("A match can only have 3 rounds");
    }

    [Fact]
    public void UpdateScores_WithIncompleteRound_ShouldThrowInvalidOperationException()
    {
        var match = new GameMatch(Guid.NewGuid(), _playerAId, _playerBId);
        var round = new GameRound(Guid.NewGuid(), match.Id, 1, Guid.NewGuid(), _testCoordinate);
        round.SetPlayers(_playerAId, _playerBId);
        match.AddRound(round);

        var act = () => match.UpdateScores(round);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot update scores for incomplete round");
    }

    [Fact]
    public void UpdateScores_WithCompletedRound_ShouldUpdateTotalPoints()
    {
        var match = new GameMatch(Guid.NewGuid(), _playerAId, _playerBId);
        var round = new GameRound(Guid.NewGuid(), match.Id, 1, Guid.NewGuid(), _testCoordinate);
        round.SetPlayers(_playerAId, _playerBId);
        match.AddRound(round);

        round.SubmitGuess(_playerAId, _testCoordinate);
        round.SubmitGuess(_playerBId, new Coordinate(40.7138, -74.0070));

        match.UpdateScores(round);

        match.PlayerATotalPoints.Should().BeGreaterThan(0);
        match.PlayerBTotalPoints.Should().BeGreaterThan(0);
    }

    [Fact]
    public void UpdateScores_After3Rounds_ShouldCompleteMatch()
    {
        var match = new GameMatch(Guid.NewGuid(), _playerAId, _playerBId);

        for (int i = 1; i <= 3; i++)
        {
            var round = new GameRound(Guid.NewGuid(), match.Id, i, Guid.NewGuid(), _testCoordinate);
            round.SetPlayers(_playerAId, _playerBId);
            match.AddRound(round);

            round.SubmitGuess(_playerAId, _testCoordinate);
            round.SubmitGuess(_playerBId, _testCoordinate);
            match.UpdateScores(round);
        }

        match.IsCompleted.Should().BeTrue();
        match.EndedAt.Should().NotBeNull();
    }

    [Fact]
    public void UpdateScores_WhenPlayerAWins_ShouldSetWinnerAndLoser()
    {
        var match = new GameMatch(Guid.NewGuid(), _playerAId, _playerBId);

        for (int i = 1; i <= 3; i++)
        {
            var round = new GameRound(Guid.NewGuid(), match.Id, i, Guid.NewGuid(), _testCoordinate);
            round.SetPlayers(_playerAId, _playerBId);
            match.AddRound(round);

            round.SubmitGuess(_playerAId, _testCoordinate);
            round.SubmitGuess(_playerBId, new Coordinate(0, 0));
            match.UpdateScores(round);
        }

        match.WinnerId.Should().Be(_playerAId);
        match.LoserId.Should().Be(_playerBId);
        match.PlayerATotalPoints.Should().BeGreaterThan(match.PlayerBTotalPoints);
    }

    [Fact]
    public void UpdateScores_WhenPlayerBWins_ShouldSetWinnerAndLoser()
    {
        var match = new GameMatch(Guid.NewGuid(), _playerAId, _playerBId);

        for (int i = 1; i <= 3; i++)
        {
            var round = new GameRound(Guid.NewGuid(), match.Id, i, Guid.NewGuid(), _testCoordinate);
            round.SetPlayers(_playerAId, _playerBId);
            match.AddRound(round);

            round.SubmitGuess(_playerAId, new Coordinate(0, 0));
            round.SubmitGuess(_playerBId, _testCoordinate);
            match.UpdateScores(round);
        }

        match.WinnerId.Should().Be(_playerBId);
        match.LoserId.Should().Be(_playerAId);
        match.PlayerBTotalPoints.Should().BeGreaterThan(match.PlayerATotalPoints);
    }

    [Fact]
    public void UpdateScores_WhenTie_ShouldNotSetWinnerOrLoser()
    {
        var match = new GameMatch(Guid.NewGuid(), _playerAId, _playerBId);

        for (int i = 1; i <= 3; i++)
        {
            var round = new GameRound(Guid.NewGuid(), match.Id, i, Guid.NewGuid(), _testCoordinate);
            round.SetPlayers(_playerAId, _playerBId);
            match.AddRound(round);

            round.SubmitGuess(_playerAId, _testCoordinate);
            round.SubmitGuess(_playerBId, _testCoordinate);
            match.UpdateScores(round);
        }

        match.WinnerId.Should().BeNull();
        match.LoserId.Should().BeNull();
        match.PlayerATotalPoints.Should().Be(match.PlayerBTotalPoints);
    }

    [Fact]
    public void GetCurrentRound_WithNoRounds_ShouldReturnNull()
    {
        var match = new GameMatch(Guid.NewGuid(), _playerAId, _playerBId);

        var currentRound = match.GetCurrentRound();

        currentRound.Should().BeNull();
    }

    [Fact]
    public void GetCurrentRound_WithIncompleteRound_ShouldReturnThatRound()
    {
        var match = new GameMatch(Guid.NewGuid(), _playerAId, _playerBId);
        var round = new GameRound(Guid.NewGuid(), match.Id, 1, Guid.NewGuid(), _testCoordinate);
        match.AddRound(round);

        var currentRound = match.GetCurrentRound();

        currentRound.Should().Be(round);
    }

    [Fact]
    public void GetCurrentRound_WithAllRoundsCompleted_ShouldReturnNull()
    {
        var match = new GameMatch(Guid.NewGuid(), _playerAId, _playerBId);

        for (int i = 1; i <= 3; i++)
        {
            var round = new GameRound(Guid.NewGuid(), match.Id, i, Guid.NewGuid(), _testCoordinate);
            round.SetPlayers(_playerAId, _playerBId);
            match.AddRound(round);

            round.SubmitGuess(_playerAId, _testCoordinate);
            round.SubmitGuess(_playerBId, _testCoordinate);
            match.UpdateScores(round);
        }

        var currentRound = match.GetCurrentRound();

        currentRound.Should().BeNull();
    }

    [Fact]
    public void GetCompletedRoundsCount_WithNoRounds_ShouldReturnZero()
    {
        var match = new GameMatch(Guid.NewGuid(), _playerAId, _playerBId);

        var count = match.GetCompletedRoundsCount();

        count.Should().Be(0);
    }

    [Fact]
    public void GetCompletedRoundsCount_WithSomeCompleted_ShouldReturnCorrectCount()
    {
        var match = new GameMatch(Guid.NewGuid(), _playerAId, _playerBId);

        for (int i = 1; i <= 2; i++)
        {
            var round = new GameRound(Guid.NewGuid(), match.Id, i, Guid.NewGuid(), _testCoordinate);
            round.SetPlayers(_playerAId, _playerBId);
            match.AddRound(round);

            round.SubmitGuess(_playerAId, _testCoordinate);
            round.SubmitGuess(_playerBId, _testCoordinate);
        }

        var count = match.GetCompletedRoundsCount();

        count.Should().Be(2);
    }
}
