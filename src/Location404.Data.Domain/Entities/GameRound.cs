using Location404.Data.Domain.ValueObjects;

namespace Location404.Data.Domain.Entities;

/// <summary>
/// Represents a single round within a match
/// </summary>
public class GameRound
{
    private GameRound() { }

    public GameRound(
        Guid id,
        Guid matchId,
        int roundNumber,
        Guid locationId,
        Coordinate correctAnswer)
    {
        Id = id;
        MatchId = matchId;
        RoundNumber = roundNumber;
        LocationId = locationId;
        CorrectAnswer = correctAnswer;
    }

    public Guid Id { get; private set; }
    public Guid MatchId { get; private set; }
    public int RoundNumber { get; private set; }

    // Location data
    public Guid LocationId { get; private set; }
    public Coordinate CorrectAnswer { get; private set; } = null!;

    // Player A guess
    public Guid PlayerAId { get; private set; }
    public Coordinate? PlayerAGuess { get; private set; }
    public double? PlayerADistance { get; private set; }
    public int? PlayerAPoints { get; private set; }

    // Player B guess
    public Guid PlayerBId { get; private set; }
    public Coordinate? PlayerBGuess { get; private set; }
    public double? PlayerBDistance { get; private set; }
    public int? PlayerBPoints { get; private set; }

    public DateTime StartedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? EndedAt { get; private set; }
    public bool IsCompleted { get; private set; }

    public void SetPlayers(Guid playerAId, Guid playerBId)
    {
        PlayerAId = playerAId;
        PlayerBId = playerBId;
    }

    public void SubmitGuess(Guid playerId, Coordinate guess)
    {
        if (IsCompleted)
            throw new InvalidOperationException("Round is already completed");

        var distance = CorrectAnswer.CalculateDistanceInKm(guess);
        var points = CalculatePoints(distance);

        if (playerId == PlayerAId)
        {
            PlayerAGuess = guess;
            PlayerADistance = distance;
            PlayerAPoints = points;
        }
        else if (playerId == PlayerBId)
        {
            PlayerBGuess = guess;
            PlayerBDistance = distance;
            PlayerBPoints = points;
        }
        else
        {
            throw new ArgumentException("Player is not part of this round");
        }

        if (PlayerAGuess != null && PlayerBGuess != null)
        {
            CompleteRound();
        }
    }

    private void CompleteRound()
    {
        IsCompleted = true;
        EndedAt = DateTime.UtcNow;
    }

    private static int CalculatePoints(double distanceKm)
    {
        const double maxScore = 5000.0;
        const double scaleFactor = 2000.0;

        var score = maxScore * Math.Exp(-distanceKm / scaleFactor);
        return Math.Max(0, (int)Math.Round(score));
    }
}
