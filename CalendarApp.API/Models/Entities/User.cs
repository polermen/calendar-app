using System;
using System.Collections.Generic;

namespace CalendarApp.API.Models.Entities;

public partial class User
{
    public int UserId { get; set; }

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<CalendarShare> CalendarShareOwners { get; set; } = new List<CalendarShare>();

    public virtual ICollection<CalendarShare> CalendarShareSpectatorUsers { get; set; } = new List<CalendarShare>();

    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    public virtual ICollection<TaskEntity> Tasks { get; set; } = new List<TaskEntity>();

    public virtual ICollection<TodoList> TodoLists { get; set; } = new List<TodoList>();
}
