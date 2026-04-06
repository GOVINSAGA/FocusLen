using System.Security.Claims;
using FocusTrack.Api.DTOs;
using FocusTrack.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FocusTrack.Api.Controllers;

[ApiController]
[Route("api/limits")]
[Authorize]
public class LimitsController : ControllerBase
{
    private readonly LimitsService _limitsService;
    private readonly ILogger<LimitsController> _logger;

    public LimitsController(LimitsService limitsService, ILogger<LimitsController> logger)
    {
        _limitsService = limitsService;
        _logger = logger;
    }

    private Guid? GetUserId()
    {
        var raw = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return Guid.TryParse(raw, out var id) ? id : null;
    }

    /// <summary>Lists all configured limits with today's usage and exceeded status.</summary>
    [HttpGet("alerts")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAlerts()
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized(new { error = "Invalid token claims." });

        var alerts = await _limitsService.GetAlertsAsync(userId.Value);
        return Ok(alerts);
    }

    /// <summary>Lists all configured limits (no usage enrichment).</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetLimits()
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized(new { error = "Invalid token claims." });

        var limits = await _limitsService.GetLimitsAsync(userId.Value);
        return Ok(limits);
    }

    /// <summary>Creates or updates a daily app limit.</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SetLimit([FromBody] SetLimitDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        if (string.IsNullOrWhiteSpace(dto.AppName))
            return BadRequest(new { error = "AppName is required." });
        if (dto.DailyLimitMinutes <= 0)
            return BadRequest(new { error = "DailyLimitMinutes must be greater than zero." });

        var userId = GetUserId();
        if (userId is null) return Unauthorized(new { error = "Invalid token claims." });

        var result = await _limitsService.SetLimitAsync(userId.Value, dto);
        return Ok(result);
    }

    /// <summary>Deletes a configured limit by app name.</summary>
    [HttpDelete("{appName}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteLimit(string appName)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized(new { error = "Invalid token claims." });

        var deleted = await _limitsService.DeleteLimitAsync(userId.Value, appName);
        return deleted ? NoContent() : NotFound(new { error = $"No limit found for '{appName}'." });
    }
}
