using System;
using System.Collections.Generic;

namespace CalendarApp.API.Models.Entities;

public partial class TodoItem
{
    public int TodoItemId { get; set; }

    public int TodoListId { get; set; }

    public string Text { get; set; } = null!;

    public bool IsCompleted { get; set; }

    public int Priority { get; set; }

    public int OrderIndex { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual TodoList TodoList { get; set; } = null!;
}
