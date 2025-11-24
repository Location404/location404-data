using FluentAssertions;
using Location404.Data.Domain.Entities;
using Location404.Data.Domain.ValueObjects;

namespace Location404.Data.Domain.UnitTests.Entities;

public class PlayerStatsTests
{
    private readonly Guid _playerId = Guid.NewGuid();
    private readonly Coordinate _testCoordinate = new(40.7128, -74.0060);

    [Fact]
    public void Constructor_WithPlayerId_ShouldInitializeDefaults()
    {
        var stats = new PlayerStats(_playerId);

        stats.PlayerId.Should().Be(_playerId);
        stats.TotalMatches.Should().Be(0);
        stats.Wins.Should().Be(0);
        stats.Losses.Should().Be(0);
        stats.Draws.Should().Be(0);
        stats.TotalRoundsPlayed.Should().Be(0);
        stats.TotalPoints.Should().Be(0);
        stats.HighestScore.Should().Be(0);
        stats.AveragePointsPerRound.Should().Be(0);
        stats.TotalDistanceErrorKm.Should().Be(0);
        stats.AverageDistanceErrorKm.Should().Be(0);
        stats.RankingPoints.Should().Be(1000);
        stats.LastMatchAt.Should().BeNull();
    }

    [Fact]
    public void UpdateAfterMatch_WhenPlayerWins_ShouldIncrementWinsAndRankingPoints()
    {
        var stats = new PlayerStats(_playerId);
        var opponentId = Guid.NewGuid();
        var match = CreateCompletedMatch(_playerId, opponentId, playerAWins: true);
        var rounds = match.Rounds;

        stats.UpdateAfterMatch(match, rounds);

        stats.TotalMatches.Should().Be(1);
        stats.Wins.Should().Be(1);
        stats.Losses.Should().Be(0);
        stats.Draws.Should().Be(0);
        stats.RankingPoints.Should().Be(1025);
        stats.LastMatchAt.Should().NotBeNull();
    }

    [Fact]
    public void UpdateAfterMatch_WhenPlayerLoses_ShouldIncrementLossesAndDecreaseRankingPoints()
    {
        var stats = new PlayerStats(_playerId);
        var opponentId = Guid.NewGuid();
        var match = CreateCompletedMatch(_playerId, opponentId, playerAWins: false);
        var rounds = match.Rounds;

        stats.UpdateAfterMatch(match, rounds);

        stats.TotalMatches.Should().Be(1);
        stats.Wins.Should().Be(0);
        stats.Losses.Should().Be(1);
        stats.Draws.Should().Be(0);
        stats.RankingPoints.Should().Be(990);
    }

    [Fact]
    public void UpdateAfterMatch_WhenDraw_ShouldIncrementDrawsAndAddSmallBonus()
    {
        var stats = new PlayerStats(_playerId);
        var opponentId = Guid.NewGuid();
        var match = CreateCompletedMatch(_playerId, opponentId, draw: true);
        var rounds = match.Rounds;

        stats.UpdateAfterMatch(match, rounds);

        stats.TotalMatches.Should().Be(1);
        stats.Wins.Should().Be(0);
        stats.Losses.Should().Be(0);
        stats.Draws.Should().Be(1);
        stats.RankingPoints.Should().Be(1005);
    }

    [Fact]
    public void UpdateAfterMatch_ShouldUpdateRoundStatistics()
    {
        var stats = new PlayerStats(_playerId);
        var opponentId = Guid.NewGuid();
        var match = CreateCompletedMatch(_playerId, opponentId, playerAWins: true);
        var rounds = match.Rounds;

        stats.UpdateAfterMatch(match, rounds);

        stats.TotalRoundsPlayed.Should().Be(3);
        stats.TotalPoints.Should().BeGreaterThan(0);
        stats.TotalDistanceErrorKm.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public void UpdateAfterMatch_ShouldUpdateHighestScore()
    {
        var stats = new PlayerStats(_playerId);
        var opponentId = Guid.NewGuid();
        var match = CreateCompletedMatch(_playerId, opponentId, playerAWins: true);
        var rounds = match.Rounds;

        stats.UpdateAfterMatch(match, rounds);

        stats.HighestScore.Should().BeGreaterThan(0);
    }

    [Fact]
    public void UpdateAfterMatch_ShouldCalculateAverages()
    {
        var stats = new PlayerStats(_playerId);
        var opponentId = Guid.NewGuid();
        var match = CreateCompletedMatch(_playerId, opponentId, playerAWins: true);
        var rounds = match.Rounds;

        stats.UpdateAfterMatch(match, rounds);

        stats.AveragePointsPerRound.Should().BeGreaterThan(0);
        stats.AverageDistanceErrorKm.Should().BeGreaterThanOrEqualTo(0);
        stats.AveragePointsPerRound.Should().Be((double)stats.TotalPoints / stats.TotalRoundsPlayed);
        stats.AverageDistanceErrorKm.Should().Be(stats.TotalDistanceErrorKm / stats.TotalRoundsPlayed);
    }

    [Fact]
    public void UpdateAfterMatch_MultipleMatches_ShouldAccumulateStats()
    {
        var stats = new PlayerStats(_playerId);
        var opponentId = Guid.NewGuid();

        var match1 = CreateCompletedMatch(_playerId, opponentId, playerAWins: true);
        stats.UpdateAfterMatch(match1, match1.Rounds);

        var match2 = CreateCompletedMatch(_playerId, opponentId, playerAWins: false);
        stats.UpdateAfterMatch(match2, match2.Rounds);

        stats.TotalMatches.Should().Be(2);
        stats.Wins.Should().Be(1);
        stats.Losses.Should().Be(1);
        stats.TotalRoundsPlayed.Should().Be(6);
        stats.RankingPoints.Should().Be(1015);
    }

    [Fact]
    public void UpdateAfterMatch_RankingPoints_ShouldNotGoBelowZero()
    {
        var stats = new PlayerStats(_playerId);
        var opponentId = Guid.NewGuid();

        for (int i = 0; i < 150; i++)
        {
            var match = CreateCompletedMatch(_playerId, opponentId, playerAWins: false);
            stats.UpdateAfterMatch(match, match.Rounds);
        }

        stats.RankingPoints.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public void GetWinRate_WithNoMatches_ShouldReturnZero()
    {
        var stats = new PlayerStats(_playerId);

        var winRate = stats.GetWinRate();

        winRate.Should().Be(0);
    }

    [Fact]
    public void GetWinRate_With1Win2Matches_ShouldReturn50Percent()
    {
        var stats = new PlayerStats(_playerId);
        var opponentId = Guid.NewGuid();

        var match1 = CreateCompletedMatch(_playerId, opponentId, playerAWins: true);
        stats.UpdateAfterMatch(match1, match1.Rounds);

        var match2 = CreateCompletedMatch(_playerId, opponentId, playerAWins: false);
        stats.UpdateAfterMatch(match2, match2.Rounds);

        var winRate = stats.GetWinRate();

        winRate.Should().Be(50);
    }

    [Fact]
    public void GetWinRate_WithAllWins_ShouldReturn100Percent()
    {
        var stats = new PlayerStats(_playerId);
        var opponentId = Guid.NewGuid();

        for (int i = 0; i < 5; i++)
        {
            var match = CreateCompletedMatch(_playerId, opponentId, playerAWins: true);
            stats.UpdateAfterMatch(match, match.Rounds);
        }

        var winRate = stats.GetWinRate();

        winRate.Should().Be(100);
    }

    [Fact]
    public void UpdateAfterMatch_AsPlayerB_ShouldWorkCorrectly()
    {
        var stats = new PlayerStats(_playerId);
        var opponentId = Guid.NewGuid();
        var match = CreateCompletedMatch(opponentId, _playerId, playerAWins: true);
        var rounds = match.Rounds;

        stats.UpdateAfterMatch(match, rounds);

        stats.TotalMatches.Should().Be(1);
        stats.Losses.Should().Be(1);
        stats.TotalRoundsPlayed.Should().Be(3);
    }

    private GameMatch CreateCompletedMatch(Guid playerAId, Guid playerBId, bool playerAWins = false, bool draw = false)
    {
        var match = new GameMatch(Guid.NewGuid(), playerAId, playerBId);

        for (int i = 1; i <= 3; i++)
        {
            var round = new GameRound(Guid.NewGuid(), match.Id, i, Guid.NewGuid(), _testCoordinate);
            round.SetPlayers(playerAId, playerBId);
            match.AddRound(round);

            if (draw)
            {
                round.SubmitGuess(playerAId, _testCoordinate);
                round.SubmitGuess(playerBId, _testCoordinate);
            }
            else if (playerAWins)
            {
                round.SubmitGuess(playerAId, _testCoordinate);
                round.SubmitGuess(playerBId, new Coordinate(0, 0));
            }
            else
            {
                round.SubmitGuess(playerAId, new Coordinate(0, 0));
                round.SubmitGuess(playerBId, _testCoordinate);
            }

            match.UpdateScores(round);
        }

        return match;
    }
}
