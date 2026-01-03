using System.Net;
using System.Net.Mail;

namespace CalendarApp.MessageConsumer.Services;

public class EmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;
    private readonly string _smtpHost;
    private readonly int _smtpPort;
    private readonly string _smtpUsername;
    private readonly string _smtpPassword;
    private readonly string _fromEmail;
    private readonly string _fromName;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;

        // Railway adds trailing spaces to variable names, so try both with and without
        _smtpHost = Environment.GetEnvironmentVariable("Email__SmtpHost")
            ?? Environment.GetEnvironmentVariable("Email__SmtpHost ")
            ?? configuration["Email:SmtpHost"] ?? "smtp.gmail.com";
        _smtpPort = int.Parse(Environment.GetEnvironmentVariable("Email__SmtpPort")
            ?? Environment.GetEnvironmentVariable("Email__SmtpPort ")
            ?? configuration["Email:SmtpPort"] ?? "587");
        _smtpUsername = Environment.GetEnvironmentVariable("Email__SmtpUsername")
            ?? Environment.GetEnvironmentVariable("Email__SmtpUsername ")
            ?? configuration["Email:SmtpUsername"] ?? "";
        _smtpPassword = Environment.GetEnvironmentVariable("Email__SmtpPassword")
            ?? Environment.GetEnvironmentVariable("Email__SmtpPassword ")
            ?? configuration["Email:SmtpPassword"] ?? "";
        _fromEmail = Environment.GetEnvironmentVariable("Email__FromEmail")
            ?? Environment.GetEnvironmentVariable("Email__FromEmail ")
            ?? configuration["Email:FromEmail"] ?? _smtpUsername;
        _fromName = Environment.GetEnvironmentVariable("Email__FromName")
            ?? Environment.GetEnvironmentVariable("Email__FromName ")
            ?? configuration["Email:FromName"] ?? "Calendar App";

        _logger.LogInformation("[DEBUG] Email Config - Host: {Host}, Port: {Port}, Username: {User}, FromEmail: {FromEmail}",
            _smtpHost, _smtpPort, _smtpUsername, _fromEmail);
    }

    public async Task SendWelcomeEmailAsync(string toEmail, string username)
    {
        try
        {
            var subject = "Welcome to Calendar App!";
            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <h2 style='color: #6366f1;'>Welcome to Calendar App, {username}!</h2>
                    <p>Thank you for registering with us. We're excited to have you on board!</p>
                    <p>You can now:</p>
                    <ul>
                        <li>Create and manage your tasks and events</li>
                        <li>Organize your todos by day, week, month, or year</li>
                        <li>Share your calendar with others</li>
                        <li>View calendars shared with you</li>
                    </ul>
                    <p>Get started by logging into your account and creating your first event!</p>
                    <br>
                    <p>Best regards,<br>The Calendar App Team</p>
                </body>
                </html>";

            await SendEmailAsync(toEmail, subject, body);
            _logger.LogInformation("Welcome email sent to {Email}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send welcome email to {Email}", toEmail);
        }
    }

    public async Task SendCalendarSharedEmailAsync(string toEmail, string ownerUsername, string ownerEmail)
    {
        try
        {
            var subject = $"{ownerUsername} shared their calendar with you!";
            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <h2 style='color: #6366f1;'>New Calendar Shared With You!</h2>
                    <p><strong>{ownerUsername}</strong> ({ownerEmail}) has shared their calendar with you.</p>
                    <p>You can now view their events and todos in read-only mode.</p>
                    <p>To access the shared calendar:</p>
                    <ol>
                        <li>Log into your Calendar App account</li>
                        <li>Click on the 'Spectate' button</li>
                        <li>Select {ownerUsername}'s calendar from the list</li>
                    </ol>
                    <br>
                    <p>Best regards,<br>The Calendar App Team</p>
                </body>
                </html>";

            await SendEmailAsync(toEmail, subject, body);
            _logger.LogInformation("Calendar shared notification sent to {Email}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send calendar shared email to {Email}", toEmail);
        }
    }

    private async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
    {
        if (string.IsNullOrEmpty(_smtpUsername) || string.IsNullOrEmpty(_smtpPassword))
        {
            _logger.LogWarning("SMTP credentials not configured. Email not sent to {Email}", toEmail);
            return;
        }

        using var smtpClient = new SmtpClient(_smtpHost, _smtpPort)
        {
            Credentials = new NetworkCredential(_smtpUsername, _smtpPassword),
            EnableSsl = true
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(_fromEmail, _fromName),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true
        };

        mailMessage.To.Add(toEmail);

        await smtpClient.SendMailAsync(mailMessage);
    }
}
