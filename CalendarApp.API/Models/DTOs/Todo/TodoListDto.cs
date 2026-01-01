namespace CalendarApp.API.Models.DTOs.Todo;

public class TodoListDto
{
    public int TodoListId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Scope { get; set; } = string.Empty;
    public DateTime? ListDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<TodoItemDto> Items { get; set; } = new();
}

public class TodoItemDto
{
    public int TodoItemId { get; set; }
    public string Text { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public int Priority { get; set; }
    public int OrderIndex { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
