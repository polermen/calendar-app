namespace CalendarApp.API.Models.Messages;

public class UserRegisteredMessage
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime RegisteredAt { get; set; }
}
