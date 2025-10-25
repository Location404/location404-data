namespace GeoDataService.Application.DTOs.Responses;

/// <summary>
/// Player statistics response
/// </summary>
public record PlayerStatsResponse(
    Guid PlayerId,
    int TotalMatches,
    int Wins,
    int Losses,
    int Draws,
    double WinRate,
    int TotalRoundsPlayed,
    int TotalPoints,
    int HighestScore,
    double AveragePointsPerRound,
    double AverageDistanceErrorKm,
    int RankingPoints,
    DateTime? LastMatchAt
);
