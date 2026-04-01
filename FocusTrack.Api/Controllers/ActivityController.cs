using System.Security.Claims;
using FocusTrack.Api.DTOs;
using FocusTrack.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FocusTrack.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ActivityController : ControllerBase
{
    private readonly ActivityIngestionService _ingestionService;
    private readonly ILogger<ActivityController> _logger;

    public ActivityController(ActivityIngestionService ingestionService, ILogger<ActivityController> logger)
    {
        _ingestionService = ingestionService;
        _logger = logger;
    }

    /// <summary>Logs an activity metric from an agent.</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> PostActivity([FromBody] ActivityPayloadDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
        {
            _logger.LogWarning("Rejecting payload: Could not extract UserId from claims.");
            return Unauthorized(new { error = "Invalid token claims." });
        }

        await _ingestionService.ProcessActivityAsync(dto, userId);

        return Accepted(new { message = "Activity logged." });
    }
}
