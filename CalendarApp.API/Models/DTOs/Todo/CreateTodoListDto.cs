using System.ComponentModel.DataAnnotations;

namespace CalendarApp.API.Models.DTOs.Todo;

public class CreateTodoListDto
{
    [Required(ErrorMessage = "Title is required")]
    [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Scope is required")]
    public string Scope { get; set; } = string.Empty; // Day, Week, Month, Year

    public DateTime? ListDate { get; set; }
}
