namespace Location404.Data.Domain.Entities;

/// <summary>
/// Represents a complete game match between two players
/// </summary>
public class GameMatch
{
    // EF Core constructor
    private GameMatch() { }

    public GameMatch(Guid id, Guid playerAId, Guid playerBId)
    {
        Id = id;
        PlayerAId = playerAId;
        PlayerBId = playerBId;
    }

    public Guid Id { get; private set; }
    public Guid PlayerAId { get; private set; }
    public Guid PlayerBId { get; private set; }

    public int PlayerATotalPoints { get; private set; } = 0;
    public int PlayerBTotalPoints { get; private set; } = 0;

    public Guid? WinnerId { get; private set; }
    public Guid? LoserId { get; private set; }

    public List<GameRound> Rounds { get; private set; } = [];

    public DateTime StartedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? EndedAt { get; private set; }
    public bool IsCompleted { get; private set; }

    public void AddRound(GameRound round)
    {
        if (Rounds.Count >= 3)
            throw new InvalidOperationException("A match can only have 3 rounds");

        if (IsCompleted)
            throw new InvalidOperationException("Cannot add rounds to a completed match");

        Rounds.Add(round);
    }

    public void UpdateScores(GameRound round)
    {
        if (!round.IsCompleted)
            throw new InvalidOperationException("Cannot update scores for incomplete round");

        PlayerATotalPoints += round.PlayerAPoints ?? 0;
        PlayerBTotalPoints += round.PlayerBPoints ?? 0;

        // Check if match should end (3 rounds completed)
        if (Rounds.Count(r => r.IsCompleted) >= 3)
        {
            EndMatch();
        }
    }

    private void EndMatch()
    {
        IsCompleted = true;
        EndedAt = DateTime.UtcNow;

        // Determine winner
        if (PlayerATotalPoints > PlayerBTotalPoints)
        {
            WinnerId = PlayerAId;
            LoserId = PlayerBId;
        }
        else if (PlayerBTotalPoints > PlayerATotalPoints)
        {
            WinnerId = PlayerBId;
            LoserId = PlayerAId;
        }
        // If tie, both remain null
    }

    public GameRound? GetCurrentRound()
    {
        return Rounds.FirstOrDefault(r => !r.IsCompleted);
    }

    public int GetCompletedRoundsCount()
    {
        return Rounds.Count(r => r.IsCompleted);
    }
}
