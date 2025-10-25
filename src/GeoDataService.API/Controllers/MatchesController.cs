using GeoDataService.Application.Common.Interfaces;
using GeoDataService.Application.DTOs.Events;
using Microsoft.AspNetCore.Mvc;

namespace GeoDataService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MatchesController(IMatchService matchService) : ControllerBase
{
    private readonly IMatchService _matchService = matchService;

    [HttpPost("ended")]
    public async Task<IActionResult> ProcessMatchEnded([FromBody] GameMatchEndedEventDto eventDto)
    {
        try
        {
            await _matchService.ProcessMatchEndedEventAsync(eventDto);
            return Ok(new { message = "Match processed successfully", matchId = eventDto.MatchId });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error processing match", error = ex.Message });
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var match = await _matchService.GetMatchByIdAsync(id);

        if (match == null)
            return NotFound(new { message = $"Match {id} not found" });

        return Ok(match);
    }

    [HttpGet("player/{playerId:guid}")]
    public async Task<IActionResult> GetPlayerMatches(
        Guid playerId,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 20)
    {
        var matches = await _matchService.GetPlayerMatchesAsync(playerId, skip, take);
        return Ok(matches);
    }
}
