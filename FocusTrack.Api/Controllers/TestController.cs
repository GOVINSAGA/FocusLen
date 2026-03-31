using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FocusTrack.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    /// <summary>
    /// A protected endpoint for acceptance-criteria validation.
    /// Requires a valid JWT Bearer token.
    /// </summary>
    [HttpGet("protected")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult GetProtected()
    {
        var email = User.FindFirstValue(ClaimTypes.Email)
                    ?? User.FindFirstValue("email")
                    ?? "unknown";

        return Ok(new
        {
            message = "✅ You have successfully hit a protected endpoint!",
            authenticatedAs = email,
            timestamp = DateTime.UtcNow
        });
    }
}
