using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace FocusTrack.DesktopAgent;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    
    // State Tracking
    private string _currentProcessName = string.Empty;
    private string _currentWindowTitle = string.Empty;
    private DateTime _activityStartTime = DateTime.UtcNow;
    private string _jwtToken = string.Empty;

    public Worker(ILogger<Worker> logger, IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Agent Authorizing...");
        await AuthorizeAgentAsync(stoppingToken);

        _logger.LogInformation("Desktop Agent acting on behalf of configured user.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                CheckForegroundWindow();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking foreground window");
            }

            await Task.Delay(5000, stoppingToken);
        }

        // Flush last activity before shutting down
        if (!string.IsNullOrEmpty(_currentProcessName))
        {
            await FlushActivityAsync(_currentProcessName, _currentWindowTitle, _activityStartTime);
        }
    }

    private void CheckForegroundWindow()
    {
        var hWnd = NativeMethods.GetForegroundWindow();
        if (hWnd == IntPtr.Zero) return;

        NativeMethods.GetWindowThreadProcessId(hWnd, out uint processId);
        
        var sb = new StringBuilder(512);
        NativeMethods.GetWindowText(hWnd, sb, sb.Capacity);
        var windowTitle = sb.ToString();

        string processName = "Unknown";
        try
        {
            var proc = Process.GetProcessById((int)processId);
            processName = proc.ProcessName;
        }
        catch { /* Process might have exited */ }

        // Ignore web browsers; the Chrome extension handles them
        if (processName.Equals("chrome", StringComparison.OrdinalIgnoreCase) || 
            processName.Equals("msedge", StringComparison.OrdinalIgnoreCase))
        {
            if (!string.IsNullOrEmpty(_currentProcessName))
            {
                _ = FlushActivityAsync(_currentProcessName, _currentWindowTitle, _activityStartTime);
                _currentProcessName = string.Empty;
                _currentWindowTitle = string.Empty;
            }
            return;
        }

        if (processName != _currentProcessName || windowTitle != _currentWindowTitle)
        {
            if (!string.IsNullOrEmpty(_currentProcessName))
            {
                _ = FlushActivityAsync(_currentProcessName, _currentWindowTitle, _activityStartTime);
            }

            _currentProcessName = processName;
            _currentWindowTitle = windowTitle;
            _activityStartTime = DateTime.UtcNow;
        }
    }

    private async Task FlushActivityAsync(string procName, string procTitle, DateTime start)
    {
        var duration = (int)(DateTime.UtcNow - start).TotalSeconds;
        if (duration < 1 || string.IsNullOrEmpty(_jwtToken)) return;

        var payload = new 
        {
            AppOrDomain = procName,
            WindowTitle = string.IsNullOrWhiteSpace(procTitle) ? procName : procTitle,
            DurationSeconds = duration,
            Source = "Desktop",
            Timestamp = DateTime.UtcNow
        };

        var client = _httpClientFactory.CreateClient("FocusTrackApi");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _jwtToken);

        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        try
        {
            var response = await client.PostAsync("api/activity", content);
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Logged Desktop Activity: {AgentName} [{Duration}s]", procName, duration);
            }
            else
            {
                _logger.LogWarning("Failed to log activity. Status Code: {StatusCode}", response.StatusCode);
                
                // If token expired, trigger re-auth
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    _jwtToken = string.Empty;
                    await AuthorizeAgentAsync(CancellationToken.None);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "HTTP Error sending generic activity.");
        }
    }

    private async Task AuthorizeAgentAsync(CancellationToken token)
    {
        var email = _configuration["AgentAuth:Email"];
        var password = _configuration["AgentAuth:Password"];

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            _logger.LogError("Agent credentials missing in appsettings.json. Telemetry disabled.");
            return;
        }

        var client = _httpClientFactory.CreateClient("FocusTrackApi");
        var payload = new { email, password };
        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        try
        {
            var res = await client.PostAsync("api/auth/login", content, token);
            if (res.IsSuccessStatusCode)
            {
                var body = await res.Content.ReadAsStringAsync(token);
                using var doc = JsonDocument.Parse(body);
                _jwtToken = doc.RootElement.GetProperty("token").GetString() ?? "";
                _logger.LogInformation("Agent authenticated successfully.");
            }
            else
            {
                _logger.LogError("Agent authentication failed with status: {StatusCode}", res.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Agent authentication request failed.");
        }
    }
}
