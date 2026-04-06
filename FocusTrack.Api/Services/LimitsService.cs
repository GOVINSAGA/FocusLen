using FocusTrack.Api.Data;
using FocusTrack.Api.DTOs;
using FocusTrack.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FocusTrack.Api.Services;

public class LimitsService
{
    private readonly FocusDbContext _context;
    private readonly ILogger<LimitsService> _logger;

    public LimitsService(FocusDbContext context, ILogger<LimitsService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>Returns all limits for the user, enriched with today's usage and exceeded flag.</summary>
    public async Task<List<LimitAlertDto>> GetAlertsAsync(Guid userId)
    {
        var limits = await _context.AppLimits
            .AsNoTracking()
            .Where(a => a.UserId == userId)
            .ToListAsync();

        if (limits.Count == 0) return [];

        var todayStart = DateTime.UtcNow.Date;
        var todayEnd = todayStart.AddDays(1);

        // Pull only today's sessions for this user (in-memory grouping to avoid SQLite DateOnly issues)
        var todaySessions = await _context.Sessions
            .AsNoTracking()
            .Where(s => s.UserId == userId && s.StartTime >= todayStart && s.StartTime < todayEnd)
            .ToListAsync();

        var usageByApp = todaySessions
            .GroupBy(s => s.AppName, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                g => g.Key,
                g => (int)g.Sum(s => (s.EndTime - s.StartTime).TotalMinutes),
                StringComparer.OrdinalIgnoreCase);

        var alerts = limits.Select(limit =>
        {
            usageByApp.TryGetValue(limit.AppName, out var usedMinutes);
            return new LimitAlertDto
            {
                AppName = limit.AppName,
                DailyLimitMinutes = limit.DailyLimitMinutes,
                TodayUsageMinutes = usedMinutes,
                IsExceeded = usedMinutes >= limit.DailyLimitMinutes
            };
        }).ToList();

        _logger.LogInformation("Evaluated {Count} app limits for user {UserId}", alerts.Count, userId);
        return alerts;
    }

    /// <summary>Returns the raw list of configured limits (no usage data).</summary>
    public async Task<List<AppLimitDto>> GetLimitsAsync(Guid userId)
    {
        return await _context.AppLimits
            .AsNoTracking()
            .Where(a => a.UserId == userId)
            .Select(a => new AppLimitDto
            {
                Id = a.Id,
                AppName = a.AppName,
                DailyLimitMinutes = a.DailyLimitMinutes
            })
            .ToListAsync();
    }

    /// <summary>Creates or updates the daily limit for an app. Uses upsert via find-or-create pattern.</summary>
    public async Task<AppLimitDto> SetLimitAsync(Guid userId, SetLimitDto dto)
    {
        var existing = await _context.AppLimits
            .FirstOrDefaultAsync(a => a.UserId == userId &&
                                      a.AppName.ToLower() == dto.AppName.ToLower());

        if (existing is not null)
        {
            existing.DailyLimitMinutes = dto.DailyLimitMinutes;
        }
        else
        {
            existing = new AppLimit
            {
                UserId = userId,
                AppName = dto.AppName.Trim(),
                DailyLimitMinutes = dto.DailyLimitMinutes
            };
            _context.AppLimits.Add(existing);
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Set limit: {App} = {Minutes} min for user {UserId}",
            existing.AppName, existing.DailyLimitMinutes, userId);

        return new AppLimitDto
        {
            Id = existing.Id,
            AppName = existing.AppName,
            DailyLimitMinutes = existing.DailyLimitMinutes
        };
    }

    /// <summary>Removes a configured app limit by app name.</summary>
    public async Task<bool> DeleteLimitAsync(Guid userId, string appName)
    {
        var limit = await _context.AppLimits
            .FirstOrDefaultAsync(a => a.UserId == userId &&
                                      a.AppName.ToLower() == appName.ToLower());
        if (limit is null) return false;

        _context.AppLimits.Remove(limit);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Deleted limit for {App} for user {UserId}", appName, userId);
        return true;
    }
}
