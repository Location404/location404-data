namespace Location404.Data.Application.DTOs.Events;

/// <summary>
/// Game match ended event from RabbitMQ
/// </summary>
public record GameMatchEndedEventDto(
    Guid MatchId,
    Guid PlayerAId,
    Guid PlayerBId,
    Guid? WinnerId,
    Guid? LoserId,
    int? PlayerATotalPoints,
    int? PlayerBTotalPoints,
    int? PointsEarned,
    int? PointsLost,
    DateTime StartTime,
    DateTime EndTime,
    List<GameRoundEventDto> Rounds
);
