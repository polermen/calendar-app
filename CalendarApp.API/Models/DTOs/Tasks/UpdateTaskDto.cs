using System.ComponentModel.DataAnnotations;

namespace CalendarApp.API.Models.DTOs.Tasks;

public class UpdateTaskDto
{
    [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
    public string? Title { get; set; }

    [MaxLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
    public string? Description { get; set; }

    public string? Scope { get; set; }

    public DateTime? TaskDate { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public bool? IsCompleted { get; set; }
}
