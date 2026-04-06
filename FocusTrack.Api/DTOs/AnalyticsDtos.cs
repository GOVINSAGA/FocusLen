namespace FocusTrack.Api.DTOs;

public class DailyUsageDto
{
    public string Date { get; set; } = string.Empty;
    public int TotalSeconds { get; set; }
}

public class TopAppDto
{
    public string AppName { get; set; } = string.Empty;
    public int TotalDurationSeconds { get; set; }
}
