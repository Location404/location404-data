using GeoDataService.Application.DTOs.Events;
using GeoDataService.Application.DTOs.Responses;

namespace GeoDataService.Application.Common.Interfaces;

public interface IMatchService
{
    Task ProcessMatchEndedEventAsync(GameMatchEndedEventDto eventDto, CancellationToken cancellationToken = default);
    Task<GameMatchResponse?> GetMatchByIdAsync(Guid matchId, CancellationToken cancellationToken = default);
    Task<List<GameMatchResponse>> GetPlayerMatchesAsync(Guid playerId, int skip = 0, int take = 20, CancellationToken cancellationToken = default);
}
