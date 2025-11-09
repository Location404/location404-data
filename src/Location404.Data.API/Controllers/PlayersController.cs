using Location404.Data.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Location404.Data.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PlayersController(IPlayerStatsService playerStatsService) : ControllerBase
{
    private readonly IPlayerStatsService _playerStatsService = playerStatsService;

    [HttpGet("{playerId:guid}/stats")]
    public async Task<IActionResult> GetStats(Guid playerId)
    {
        var stats = await _playerStatsService.GetPlayerStatsAsync(playerId);

        if (stats == null)
            return NotFound(new { message = $"Stats for player {playerId} not found" });

        return Ok(stats);
    }

    [HttpGet("ranking")]
    public async Task<IActionResult> GetRanking([FromQuery] int count = 10)
    {
        if (count < 1 || count > 100)
            return BadRequest(new { message = "Count must be between 1 and 100" });

        var ranking = await _playerStatsService.GetRankingAsync(count);
        return Ok(ranking);
    }
}
