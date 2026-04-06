using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace FocusTrack.Api.Services;

public class EmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration config, ILogger<EmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    /// <summary>
    /// Sends an HTML email via SMTP. Reads credentials from appsettings Smtp section.
    /// For local dev, configure Ethereal: https://ethereal.email
    /// </summary>
    public async Task SendAsync(string to, string subject, string htmlBody)
    {
        var smtpHost = _config["Smtp:Host"] ?? throw new InvalidOperationException("Smtp:Host not configured.");
        var smtpPort = int.Parse(_config["Smtp:Port"] ?? "587");
        var smtpUser = _config["Smtp:Username"] ?? throw new InvalidOperationException("Smtp:Username not configured.");
        var smtpPass = _config["Smtp:Password"] ?? throw new InvalidOperationException("Smtp:Password not configured.");
        var fromAddr = _config["Smtp:From"] ?? smtpUser;

        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(fromAddr));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = htmlBody };

        using var client = new SmtpClient();
        await client.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(smtpUser, smtpPass);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);

        _logger.LogInformation("Email sent to {To}: {Subject}", to, subject);
    }
}
