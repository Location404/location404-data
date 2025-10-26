namespace Location404.Data.Application.DTOs.Responses;

/// <summary>
/// Game match response with rounds
/// </summary>
public record GameMatchResponse(
    Guid Id,
    Guid PlayerAId,
    Guid PlayerBId,
    int PlayerATotalPoints,
    int PlayerBTotalPoints,
    Guid? WinnerId,
    Guid? LoserId,
    DateTime StartedAt,
    DateTime? EndedAt,
    bool IsCompleted,
    List<GameRoundResponse> Rounds
);

public record GameRoundResponse(
    Guid Id,
    int RoundNumber,
    Guid LocationId,
    CoordinateDto CorrectAnswer,
    Guid PlayerAId,
    CoordinateDto? PlayerAGuess,
    double? PlayerADistance,
    int? PlayerAPoints,
    Guid PlayerBId,
    CoordinateDto? PlayerBGuess,
    double? PlayerBDistance,
    int? PlayerBPoints,
    DateTime StartedAt,
    DateTime? EndedAt,
    bool IsCompleted
);
