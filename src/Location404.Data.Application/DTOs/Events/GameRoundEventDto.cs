namespace Location404.Data.Application.DTOs.Events;

/// <summary>
/// Game round data from RabbitMQ event
/// </summary>
public record GameRoundEventDto(
    Guid Id,
    Guid GameMatchId,
    int RoundNumber,
    Guid PlayerAId,
    Guid PlayerBId,
    int? PlayerAPoints,
    int? PlayerBPoints,
    CoordinateDto? GameResponse,
    CoordinateDto? PlayerAGuess,
    CoordinateDto? PlayerBGuess,
    bool GameRoundEnded
);
