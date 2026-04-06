namespace FocusTrack.Api.Models;

public class AppLimit
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string AppName { get; set; } = string.Empty;
    public int DailyLimitMinutes { get; set; }

    // Navigation property
    public User User { get; set; } = null!;
}
