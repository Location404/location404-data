using Location404.Data.Application.Common.Interfaces;
using Location404.Data.Application.DTOs;
using Location404.Data.Application.DTOs.Events;
using Location404.Data.Application.DTOs.Responses;
using Location404.Data.Domain.Entities;
using Location404.Data.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Location404.Data.Application.Services;

public class MatchService(IUnitOfWork unitOfWork, ILogger<MatchService> logger) : IMatchService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ILogger<MatchService> _logger = logger;

    public async Task ProcessMatchEndedEventAsync(GameMatchEndedEventDto eventDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var existingMatch = await _unitOfWork.Matches.GetByIdAsync(eventDto.MatchId, cancellationToken);
            if (existingMatch != null)
            {
                _logger.LogWarning("Match {MatchId} already exists, skipping", eventDto.MatchId);
                return;
            }

            var match = new GameMatch(
                eventDto.MatchId,
                eventDto.PlayerAId,
                eventDto.PlayerBId
            );

            foreach (var roundDto in eventDto.Rounds)
            {
                if (roundDto.GameResponse == null || roundDto.PlayerAGuess == null || roundDto.PlayerBGuess == null)
                    continue;

                var correctAnswer = new Coordinate(roundDto.GameResponse.X, roundDto.GameResponse.Y);
                var round = new GameRound(
                    roundDto.Id,
                    eventDto.MatchId,
                    roundDto.RoundNumber,
                    Guid.Empty, // LocationId - we don't have this from game-core-engine yet
                    correctAnswer
                );

                round.SetPlayers(eventDto.PlayerAId, eventDto.PlayerBId);
                round.SubmitGuess(eventDto.PlayerAId, new Coordinate(roundDto.PlayerAGuess.X, roundDto.PlayerAGuess.Y));
                round.SubmitGuess(eventDto.PlayerBId, new Coordinate(roundDto.PlayerBGuess.X, roundDto.PlayerBGuess.Y));

                match.AddRound(round);
                match.UpdateScores(round);
            }

            await _unitOfWork.Matches.AddAsync(match, cancellationToken);

            await UpdatePlayerStatsAsync(eventDto.PlayerAId, match, match.Rounds, cancellationToken);
            await UpdatePlayerStatsAsync(eventDto.PlayerBId, match, match.Rounds, cancellationToken);

            // SaveChangesAsync is atomic - it will commit all changes or rollback all changes automatically
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Match {MatchId} processed successfully", eventDto.MatchId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing match {MatchId}", eventDto.MatchId);
            throw;
        }
    }

    public async Task<GameMatchResponse?> GetMatchByIdAsync(Guid matchId, CancellationToken cancellationToken = default)
    {
        var match = await _unitOfWork.Matches.GetByIdAsync(matchId, cancellationToken);
        return match == null ? null : MapToResponse(match);
    }

    public async Task<List<GameMatchResponse>> GetPlayerMatchesAsync(Guid playerId, int skip = 0, int take = 20, CancellationToken cancellationToken = default)
    {
        var matches = await _unitOfWork.Matches.GetByPlayerIdAsync(playerId, skip, take, cancellationToken);
        return matches.Select(MapToResponse).ToList();
    }

    private async Task UpdatePlayerStatsAsync(Guid playerId, GameMatch match, List<GameRound> rounds, CancellationToken cancellationToken)
    {
        var stats = await _unitOfWork.PlayerStats.GetByPlayerIdAsync(playerId, cancellationToken);

        if (stats == null)
        {
            stats = new PlayerStats(playerId);
            stats.UpdateAfterMatch(match, rounds);
            await _unitOfWork.PlayerStats.AddAsync(stats, cancellationToken);
        }
        else
        {
            stats.UpdateAfterMatch(match, rounds);
            await _unitOfWork.PlayerStats.UpdateAsync(stats, cancellationToken);
        }
    }

    private static GameMatchResponse MapToResponse(GameMatch match)
    {
        return new GameMatchResponse(
            match.Id,
            match.PlayerAId,
            match.PlayerBId,
            match.PlayerATotalPoints,
            match.PlayerBTotalPoints,
            match.WinnerId,
            match.LoserId,
            match.StartedAt,
            match.EndedAt,
            match.IsCompleted,
            match.Rounds.Select(MapRoundToResponse).ToList()
        );
    }

    private static GameRoundResponse MapRoundToResponse(GameRound round)
    {
        return new GameRoundResponse(
            round.Id,
            round.RoundNumber,
            round.LocationId,
            new CoordinateDto(round.CorrectAnswer.X, round.CorrectAnswer.Y),
            round.PlayerAId,
            round.PlayerAGuess != null ? new CoordinateDto(round.PlayerAGuess.X, round.PlayerAGuess.Y) : null,
            round.PlayerADistance,
            round.PlayerAPoints,
            round.PlayerBId,
            round.PlayerBGuess != null ? new CoordinateDto(round.PlayerBGuess.X, round.PlayerBGuess.Y) : null,
            round.PlayerBDistance,
            round.PlayerBPoints,
            round.StartedAt,
            round.EndedAt,
            round.IsCompleted
        );
    }
}
