using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CalendarApp.API.Models.Entities;

[Table("Tasks")]
public class TaskEntity
{
    [Key]
    [Column("TaskId")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int TaskId { get; set; }

    [Required]
    [Column("UserId")]
    public int UserId { get; set; }

    [Required]
    [MaxLength(200)]
    [Column("Title")]
    public string Title { get; set; } = string.Empty;

    [Column("Description")]
    public string? Description { get; set; }

    [Required]
    [MaxLength(20)]
    [Column("Scope")]
    public string Scope { get; set; } = string.Empty; // Day, Week, Month, Year

    [Column("TaskDate")]
    public DateTime? TaskDate { get; set; }

    [Column("StartDate")]
    public DateTime? StartDate { get; set; }

    [Column("EndDate")]
    public DateTime? EndDate { get; set; }

    [Column("IsCompleted")]
    public bool IsCompleted { get; set; } = false;

    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("UpdatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    [ForeignKey("UserId")]
    public User User { get; set; } = null!;
}

public enum TaskScope
{
    Day,
    Week,
    Month,
    Year
}
