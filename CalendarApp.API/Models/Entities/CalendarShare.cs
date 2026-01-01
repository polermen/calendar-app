using System;
using System.Collections.Generic;

namespace CalendarApp.API.Models.Entities;

public partial class CalendarShare
{
    public int CalendarShareId { get; set; }

    public int OwnerId { get; set; }

    public string SpectatorEmail { get; set; } = null!;

    public int? SpectatorUserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool IsAccepted { get; set; }

    public virtual User Owner { get; set; } = null!;

    public virtual User? SpectatorUser { get; set; }
}
