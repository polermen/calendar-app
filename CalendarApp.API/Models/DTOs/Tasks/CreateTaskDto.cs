using System.ComponentModel.DataAnnotations;

namespace CalendarApp.API.Models.DTOs.Tasks;

public class CreateTaskDto
{
    [Required(ErrorMessage = "Title is required")]
    [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Scope is required")]
    public string Scope { get; set; } = string.Empty; // Day, Week, Month, Year

    public DateTime? TaskDate { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }
}
