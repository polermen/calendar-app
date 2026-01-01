using System.ComponentModel.DataAnnotations;

namespace CalendarApp.API.Models.DTOs.Todo;

public class UpdateTodoItemDto
{
    [MaxLength(500, ErrorMessage = "Text cannot exceed 500 characters")]
    public string? Text { get; set; }

    public bool? IsCompleted { get; set; }

    public int? Priority { get; set; }

    public int? OrderIndex { get; set; }
}
