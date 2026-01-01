using System.ComponentModel.DataAnnotations;

namespace CalendarApp.API.Models.DTOs.Todo;

public class CreateTodoItemDto
{
    [Required(ErrorMessage = "Text is required")]
    [MaxLength(500, ErrorMessage = "Text cannot exceed 500 characters")]
    public string Text { get; set; } = string.Empty;

    public int Priority { get; set; } = 0; // 0=Low, 1=Medium, 2=High
}
