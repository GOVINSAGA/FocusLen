namespace FocusTrack.Api.Models;

public class Session
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string AppName { get; set; } = string.Empty;
    public string WindowTitle { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool IsBrowser { get; set; }

    // Navigation property
    public User? User { get; set; }
}
