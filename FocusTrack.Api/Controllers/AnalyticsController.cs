using System.Security.Claims;
using FocusTrack.Api.DTOs;
using FocusTrack.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FocusTrack.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AnalyticsController : ControllerBase
{
    private readonly AnalyticsService _analyticsService;

    public AnalyticsController(AnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    private Guid GetUserId()
    {
        var idString = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return Guid.TryParse(idString, out var guid) ? guid : Guid.Empty;
    }

    [HttpGet("daily")]
    [ProducesResponseType(typeof(List<DailyUsageDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDailyUsage([FromQuery] int daysBack = 7)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var data = await _analyticsService.GetDailyUsageAsync(userId, daysBack);
        return Ok(data);
    }

    [HttpGet("top-apps")]
    [ProducesResponseType(typeof(List<TopAppDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTopApps([FromQuery] int limit = 5)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var data = await _analyticsService.GetTopAppsAsync(userId, limit);
        return Ok(data);
    }
}
