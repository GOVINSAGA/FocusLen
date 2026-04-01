using FocusTrack.Api.Data;
using FocusTrack.Api.DTOs;
using FocusTrack.Api.Models;

namespace FocusTrack.Api.Services;

public class ActivityIngestionService
{
    private readonly FocusDbContext _context;
    private readonly ILogger<ActivityIngestionService> _logger;

    public ActivityIngestionService(FocusDbContext context, ILogger<ActivityIngestionService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task ProcessActivityAsync(ActivityPayloadDto payload, Guid userId)
    {
        // Compute Start and End times
        var endTime = payload.Timestamp.ToUniversalTime();
        var startTime = endTime.AddSeconds(-payload.DurationSeconds);

        var session = new Session
        {
            UserId = userId,
            AppName = payload.AppOrDomain,
            WindowTitle = payload.WindowTitle,
            StartTime = startTime,
            EndTime = endTime,
            IsBrowser = payload.Source.Equals("Browser", StringComparison.OrdinalIgnoreCase)
        };

        _context.Sessions.Add(session);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Logged {Duration}s of activity for user {UserId} in {App}", 
            payload.DurationSeconds, userId, payload.AppOrDomain);
    }
}
