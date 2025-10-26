namespace GeoDataService.Domain.Entities;

/// <summary>
/// Aggregated statistics for a player
/// </summary>
public class PlayerStats
{
    // EF Core constructor
    private PlayerStats() { }

    public PlayerStats(Guid playerId)
    {
        PlayerId = playerId;
    }

    public Guid PlayerId { get; private set; }

    public int TotalMatches { get; private set; } = 0;
    public int Wins { get; private set; } = 0;
    public int Losses { get; private set; } = 0;
    public int Draws { get; private set; } = 0;

    public int TotalRoundsPlayed { get; private set; } = 0;
    public int TotalPoints { get; private set; } = 0;
    public int HighestScore { get; private set; } = 0;
    public double AveragePointsPerRound { get; private set; } = 0;

    public double TotalDistanceErrorKm { get; private set; } = 0;
    public double AverageDistanceErrorKm { get; private set; } = 0;

    public int RankingPoints { get; private set; } = 1000; // ELO-style starting points

    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? LastMatchAt { get; private set; }

    public void UpdateAfterMatch(GameMatch match, List<GameRound> rounds)
    {
        TotalMatches++;
        LastMatchAt = DateTime.UtcNow;

        var isPlayerA = match.PlayerAId == PlayerId;
        var playerPoints = isPlayerA ? match.PlayerATotalPoints : match.PlayerBTotalPoints;

        // Update win/loss/draw
        if (match.WinnerId == PlayerId)
        {
            Wins++;
            RankingPoints += 25; // Win bonus
        }
        else if (match.LoserId == PlayerId)
        {
            Losses++;
            RankingPoints = Math.Max(0, RankingPoints - 10); // Loss penalty
        }
        else if (match.IsCompleted) // Draw
        {
            Draws++;
            RankingPoints += 5; // Small draw bonus
        }

        // Update round statistics
        foreach (var round in rounds)
        {
            if (!round.IsCompleted) continue;

            TotalRoundsPlayed++;

            var roundPoints = isPlayerA ? round.PlayerAPoints ?? 0 : round.PlayerBPoints ?? 0;
            var roundDistance = isPlayerA ? round.PlayerADistance ?? 0 : round.PlayerBDistance ?? 0;

            TotalPoints += roundPoints;
            TotalDistanceErrorKm += roundDistance;

            if (roundPoints > HighestScore)
                HighestScore = roundPoints;
        }

        // Recalculate averages
        if (TotalRoundsPlayed > 0)
        {
            AveragePointsPerRound = (double)TotalPoints / TotalRoundsPlayed;
            AverageDistanceErrorKm = TotalDistanceErrorKm / TotalRoundsPlayed;
        }
    }

    public double GetWinRate()
    {
        return TotalMatches > 0 ? (double)Wins / TotalMatches * 100 : 0;
    }
}
