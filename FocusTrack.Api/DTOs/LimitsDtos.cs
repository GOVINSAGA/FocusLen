namespace FocusTrack.Api.DTOs;

// ── Inbound ──────────────────────────────────────────────────────────────────

/// <summary>Request body to create or update an app limit.</summary>
public class SetLimitDto
{
    public string AppName { get; set; } = string.Empty;
    public int DailyLimitMinutes { get; set; }
}

// ── Outbound ─────────────────────────────────────────────────────────────────

/// <summary>A user's configured limit, enriched with today's usage.</summary>
public class LimitAlertDto
{
    public string AppName { get; set; } = string.Empty;
    public int DailyLimitMinutes { get; set; }
    public int TodayUsageMinutes { get; set; }
    public bool IsExceeded { get; set; }
}

/// <summary>Lightweight record of a saved limit (for the list view).</summary>
public class AppLimitDto
{
    public Guid Id { get; set; }
    public string AppName { get; set; } = string.Empty;
    public int DailyLimitMinutes { get; set; }
}
