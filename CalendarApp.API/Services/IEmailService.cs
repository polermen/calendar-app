namespace CalendarApp.API.Services;

public interface IEmailService
{
    Task SendWelcomeEmailAsync(string toEmail, string username);
    Task SendCalendarSharedEmailAsync(string toEmail, string ownerUsername, string ownerEmail);
}
