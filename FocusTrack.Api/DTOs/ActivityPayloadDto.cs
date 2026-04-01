using System.ComponentModel.DataAnnotations;

namespace FocusTrack.Api.DTOs;

public class ActivityPayloadDto
{
    [Required]
    public string AppOrDomain { get; set; } = string.Empty;

    public string WindowTitle { get; set; } = string.Empty;

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Duration must be at least 1 second.")]
    public int DurationSeconds { get; set; }

    [Required]
    public string Source { get; set; } = string.Empty; // "Browser" or "Desktop"

    [Required]
    public DateTime Timestamp { get; set; }
}
