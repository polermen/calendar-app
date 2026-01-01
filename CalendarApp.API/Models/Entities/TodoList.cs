using System;
using System.Collections.Generic;

namespace CalendarApp.API.Models.Entities;

public partial class TodoList
{
    public int TodoListId { get; set; }

    public int UserId { get; set; }

    public string Title { get; set; } = null!;

    public string Scope { get; set; } = null!;

    public DateTime? ListDate { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<TodoItem> TodoItems { get; set; } = new List<TodoItem>();

    // Alias for TodoItems to maintain compatibility
    public ICollection<TodoItem> Items => TodoItems;

    public virtual User User { get; set; } = null!;
}
