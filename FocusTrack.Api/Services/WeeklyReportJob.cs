using FocusTrack.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace FocusTrack.Api.Services;

/// <summary>
/// Hangfire background job: runs weekly and sends a focus summary email to every user.
/// Trigger manually from /hangfire dashboard or wait for the weekly CRON schedule.
/// </summary>
public class WeeklyReportJob
{
    private readonly FocusDbContext _context;
    private readonly EmailService _emailService;
    private readonly ILogger<WeeklyReportJob> _logger;

    public WeeklyReportJob(FocusDbContext context, EmailService emailService, ILogger<WeeklyReportJob> logger)
    {
        _context = context;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task ExecuteAsync()
    {
        var weekStart = DateTime.UtcNow.Date.AddDays(-7);
        var users = await _context.Users.AsNoTracking().ToListAsync();

        _logger.LogInformation("WeeklyReportJob: Processing {Count} users", users.Count);

        foreach (var user in users)
        {
            try
            {
                var sessions = await _context.Sessions
                    .AsNoTracking()
                    .Where(s => s.UserId == user.Id && s.StartTime >= weekStart)
                    .ToListAsync();

                var totalMinutes = (int)sessions.Sum(s => (s.EndTime - s.StartTime).TotalMinutes);
                var totalHours = totalMinutes / 60;
                var remainderMins = totalMinutes % 60;

                var topApps = sessions
                    .GroupBy(s => s.AppName)
                    .Select(g => new { App = g.Key, Minutes = (int)g.Sum(s => (s.EndTime - s.StartTime).TotalMinutes) })
                    .OrderByDescending(x => x.Minutes)
                    .Take(3)
                    .ToList();

                var topAppsHtml = topApps.Count > 0
                    ? string.Join("", topApps.Select((a, i) =>
                        $"<tr><td style='padding:8px 12px;border-bottom:1px solid #e5e7eb;'>{i + 1}. {a.App}</td>" +
                        $"<td style='padding:8px 12px;border-bottom:1px solid #e5e7eb;text-align:right;font-weight:600;color:#6c63ff;'>{a.Minutes} min</td></tr>"))
                    : "<tr><td colspan='2' style='padding:12px;color:#9ca3af;text-align:center;'>No activity this week</td></tr>";

                var html = $"""
                    <!DOCTYPE html>
                    <html>
                    <head><meta charset='utf-8'></head>
                    <body style='font-family:Inter,sans-serif;background:#f9fafb;margin:0;padding:24px;'>
                      <div style='max-width:560px;margin:0 auto;background:#fff;border-radius:16px;overflow:hidden;box-shadow:0 4px 20px rgba(0,0,0,.08);'>
                        <div style='background:linear-gradient(135deg,#6c63ff,#4ecca3);padding:32px;text-align:center;'>
                          <h1 style='color:#fff;margin:0;font-size:24px;'>⏱ Your Weekly Focus Report</h1>
                          <p style='color:rgba(255,255,255,.8);margin:8px 0 0;'>Week ending {DateTime.UtcNow:MMMM dd, yyyy}</p>
                        </div>
                        <div style='padding:32px;'>
                          <p style='color:#374151;'>Hi {user.Email},</p>
                          <p style='color:#6b7280;'>Here's a summary of your focused time this past week:</p>
                          <div style='background:#f3f4f6;border-radius:12px;padding:24px;text-align:center;margin:20px 0;'>
                            <div style='font-size:48px;font-weight:700;color:#6c63ff;'>{totalHours}h {remainderMins}m</div>
                            <div style='color:#9ca3af;font-size:14px;margin-top:4px;'>Total Focus Time</div>
                          </div>
                          <h3 style='color:#374151;margin:24px 0 12px;'>Top Applications</h3>
                          <table style='width:100%;border-collapse:collapse;'>
                            {topAppsHtml}
                          </table>
                          <p style='color:#9ca3af;font-size:12px;margin-top:32px;text-align:center;'>
                            FocusTrack • Your personal productivity companion
                          </p>
                        </div>
                      </div>
                    </body>
                    </html>
                    """;

                await _emailService.SendAsync(
                    to: user.Email,
                    subject: $"📊 Your FocusTrack Weekly Report — {totalHours}h {remainderMins}m focused",
                    htmlBody: html);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send weekly report to {Email}", user.Email);
                // Continue processing other users even if one fails
            }
        }

        _logger.LogInformation("WeeklyReportJob: Completed for {Count} users", users.Count);
    }
}
