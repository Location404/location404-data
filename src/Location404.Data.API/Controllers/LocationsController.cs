using Location404.Data.Application.Common.Interfaces;
using Location404.Data.Application.DTOs.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Location404.Data.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LocationsController(ILocationService locationService) : ControllerBase
{
    private readonly ILocationService _locationService = locationService;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool activeOnly = true)
    {
        var locations = await _locationService.GetAllLocationsAsync(activeOnly);
        return Ok(locations);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var location = await _locationService.GetLocationByIdAsync(id);

        if (location == null)
            return NotFound(new { message = $"Location {id} not found" });

        return Ok(location);
    }

    [HttpGet("random")]
    [AllowAnonymous]
    public async Task<IActionResult> GetRandom()
    {
        var location = await _locationService.GetRandomLocationAsync();

        if (location == null)
            return NotFound(new { message = "No active locations available" });

        return Ok(location);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateLocationRequest request)
    {
        var location = await _locationService.CreateLocationAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = location.Id }, location);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _locationService.DeleteLocationAsync(id);

        if (!result)
            return NotFound(new { message = $"Location {id} not found" });

        return NoContent();
    }
}
