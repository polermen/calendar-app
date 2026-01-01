namespace CalendarApp.API.Models.Messages;

public class CalendarSharedMessage
{
    public int ShareId { get; set; }
    public int OwnerId { get; set; }
    public string OwnerUsername { get; set; } = string.Empty;
    public string OwnerEmail { get; set; } = string.Empty;
    public string SpectatorEmail { get; set; } = string.Empty;
    public DateTime SharedAt { get; set; }
}
