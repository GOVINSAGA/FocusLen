using FocusTrack.Api.Data;
using FocusTrack.Api.DTOs;
using Microsoft.EntityFrameworkCore;

namespace FocusTrack.Api.Services;

public class AnalyticsService
{
    private readonly FocusDbContext _context;

    public AnalyticsService(FocusDbContext context)
    {
        _context = context;
    }

    public async Task<List<DailyUsageDto>> GetDailyUsageAsync(Guid userId, int daysBack = 7)
    {
        var cutoffDate = DateTime.UtcNow.Date.AddDays(-daysBack);

        // Fetch user's sessions from the DB within the time range
        var sessions = await _context.Sessions
            .AsNoTracking()
            .Where(s => s.UserId == userId && s.StartTime >= cutoffDate)
            .ToListAsync(); // Execute query securely

        // Calculate and aggregate duration in-memory since EF Core SQLite struggles with Date conversions in group by's
        var rawAggregated = sessions
            .GroupBy(s => s.StartTime.Date)
            .Select(g => new DailyUsageDto
            {
                Date = g.Key.ToString("yyyy-MM-dd"),
                TotalSeconds = g.Sum(s => (int)(s.EndTime - s.StartTime).TotalSeconds)
            })
            .OrderBy(dto => dto.Date)
            .ToList();

        // Fill in missing days with 0 seconds
        var result = new List<DailyUsageDto>();
        for (int i = daysBack; i >= 0; i--)
        {
            var targetDate = DateTime.UtcNow.Date.AddDays(-i).ToString("yyyy-MM-dd");
            var existing = rawAggregated.FirstOrDefault(r => r.Date == targetDate);
            result.Add(existing ?? new DailyUsageDto { Date = targetDate, TotalSeconds = 0 });
        }

        return result;
    }

    public async Task<List<TopAppDto>> GetTopAppsAsync(Guid userId, int limit = 5)
    {
        var sessions = await _context.Sessions
            .AsNoTracking()
            .Where(s => s.UserId == userId)
            .ToListAsync(); 

        var topApps = sessions
            .GroupBy(s => s.AppName)
            .Select(g => new TopAppDto
            {
                AppName = g.Key,
                TotalDurationSeconds = g.Sum(s => (int)(s.EndTime - s.StartTime).TotalSeconds)
            })
            .OrderByDescending(dto => dto.TotalDurationSeconds)
            .Take(limit)
            .ToList();

        return topApps;
    }
}
