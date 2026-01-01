namespace CalendarApp.API.Models.DTOs.Share;

public class CalendarShareDto
{
    public int CalendarShareId { get; set; }
    public int OwnerId { get; set; }
    public string OwnerUsername { get; set; } = string.Empty;
    public string OwnerEmail { get; set; } = string.Empty;
    public string SpectatorEmail { get; set; } = string.Empty;
    public int? SpectatorUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsAccepted { get; set; }
}

public class CreateCalendarShareDto
{
    public string SpectatorEmail { get; set; } = string.Empty;
}

public class SharedCalendarDto
{
    public int OwnerId { get; set; }
    public string OwnerUsername { get; set; } = string.Empty;
    public string OwnerEmail { get; set; } = string.Empty;
    public DateTime SharedAt { get; set; }
}
