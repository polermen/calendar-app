using CalendarApp.API.Data.Repositories.Interfaces;
using CalendarApp.API.Models.DTOs.Tasks;
using CalendarApp.API.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CalendarApp.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly ITaskRepository _taskRepository;
    private readonly ICalendarShareRepository _shareRepository;
    private readonly ILogger<TasksController> _logger;

    public TasksController(
        ITaskRepository taskRepository,
        ICalendarShareRepository shareRepository,
        ILogger<TasksController> logger)
    {
        _taskRepository = taskRepository;
        _shareRepository = shareRepository;
        _logger = logger;
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.Parse(userIdClaim!);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskDto>>> GetAllTasks()
    {
        try
        {
            var userId = GetUserId();
            var tasks = await _taskRepository.GetAllAsync(userId);

            var taskDtos = tasks.Select(t => new TaskDto
            {
                TaskId = t.TaskId,
                Title = t.Title,
                Description = t.Description,
                Scope = t.Scope,
                TaskDate = t.TaskDate,
                StartDate = t.StartDate,
                EndDate = t.EndDate,
                IsCompleted = t.IsCompleted,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt
            });

            return Ok(taskDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tasks");
            return StatusCode(500, "An error occurred while retrieving tasks");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TaskDto>> GetTask(int id)
    {
        try
        {
            var userId = GetUserId();
            var task = await _taskRepository.GetByIdAsync(id, userId);

            if (task == null)
                return NotFound();

            var taskDto = new TaskDto
            {
                TaskId = task.TaskId,
                Title = task.Title,
                Description = task.Description,
                Scope = task.Scope,
                TaskDate = task.TaskDate,
                StartDate = task.StartDate,
                EndDate = task.EndDate,
                IsCompleted = task.IsCompleted,
                CreatedAt = task.CreatedAt,
                UpdatedAt = task.UpdatedAt
            };

            return Ok(taskDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting task {TaskId}", id);
            return StatusCode(500, "An error occurred while retrieving the task");
        }
    }

    [HttpPost]
    public async Task<ActionResult<TaskDto>> CreateTask([FromBody] CreateTaskDto taskDto)
    {
        try
        {
            var userId = GetUserId();

            var task = new TaskEntity
            {
                UserId = userId,
                Title = taskDto.Title,
                Description = taskDto.Description,
                Scope = taskDto.Scope,
                TaskDate = taskDto.TaskDate,
                StartDate = taskDto.StartDate,
                EndDate = taskDto.EndDate,
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createdTask = await _taskRepository.CreateAsync(task);

            var responseDto = new TaskDto
            {
                TaskId = createdTask.TaskId,
                Title = task.Title,
                Description = task.Description,
                Scope = task.Scope,
                TaskDate = task.TaskDate,
                StartDate = task.StartDate,
                EndDate = task.EndDate,
                IsCompleted = task.IsCompleted,
                CreatedAt = task.CreatedAt,
                UpdatedAt = task.UpdatedAt
            };

            return CreatedAtAction(nameof(GetTask), new { id = createdTask.TaskId }, responseDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating task");
            return StatusCode(500, "An error occurred while creating the task");
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<TaskDto>> UpdateTask(int id, [FromBody] UpdateTaskDto taskDto)
    {
        try
        {
            var userId = GetUserId();
            var existingTask = await _taskRepository.GetByIdAsync(id, userId);

            if (existingTask == null)
                return NotFound();

            existingTask.Title = taskDto.Title ?? existingTask.Title;
            existingTask.Description = taskDto.Description ?? existingTask.Description;
            existingTask.Scope = taskDto.Scope ?? existingTask.Scope;
            existingTask.TaskDate = taskDto.TaskDate ?? existingTask.TaskDate;
            existingTask.StartDate = taskDto.StartDate ?? existingTask.StartDate;
            existingTask.EndDate = taskDto.EndDate ?? existingTask.EndDate;
            existingTask.IsCompleted = taskDto.IsCompleted ?? existingTask.IsCompleted;
            existingTask.UpdatedAt = DateTime.UtcNow;

            var updatedTask = await _taskRepository.UpdateAsync(existingTask);

            var responseDto = new TaskDto
            {
                TaskId = updatedTask.TaskId,
                Title = existingTask.Title,
                Description = existingTask.Description,
                Scope = existingTask.Scope,
                TaskDate = existingTask.TaskDate,
                StartDate = existingTask.StartDate,
                EndDate = existingTask.EndDate,
                IsCompleted = existingTask.IsCompleted,
                CreatedAt = existingTask.CreatedAt,
                UpdatedAt = existingTask.UpdatedAt
            };

            return Ok(responseDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating task {TaskId}", id);
            return StatusCode(500, "An error occurred while updating the task");
        }
    }

    [HttpPatch("{id}/complete")]
    public async Task<ActionResult> MarkTaskComplete(int id)
    {
        try
        {
            var userId = GetUserId();
            var task = await _taskRepository.GetByIdAsync(id, userId);

            if (task == null)
                return NotFound();

            task.IsCompleted = !task.IsCompleted;
            task.UpdatedAt = DateTime.UtcNow;

            await _taskRepository.UpdateAsync(task);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking task {TaskId} as complete", id);
            return StatusCode(500, "An error occurred while updating the task");
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteTask(int id)
    {
        try
        {
            var userId = GetUserId();
            var deleted = await _taskRepository.DeleteAsync(id, userId);

            if (!deleted)
                return NotFound();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting task {TaskId}", id);
            return StatusCode(500, "An error occurred while deleting the task");
        }
    }

    [HttpGet("spectate/{ownerId}")]
    public async Task<ActionResult<IEnumerable<TaskDto>>> GetSpectatedTasks(int ownerId)
    {
        try
        {
            var spectatorId = GetUserId();

            // Check if the current user has permission to spectate this calendar
            var shares = await _shareRepository.GetSharesBySpectatorIdAsync(spectatorId);
            var hasAccess = shares.Any(s => s.OwnerId == ownerId);

            if (!hasAccess)
                return Forbid();

            var tasks = await _taskRepository.GetAllAsync(ownerId);

            var taskDtos = tasks.Select(t => new TaskDto
            {
                TaskId = t.TaskId,
                Title = t.Title,
                Description = t.Description,
                TaskDate = t.TaskDate,
                StartDate = t.StartDate,
                EndDate = t.EndDate,
                Scope = t.Scope,
                IsCompleted = t.IsCompleted,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt
            });

            return Ok(taskDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting spectated tasks for owner {OwnerId}", ownerId);
            return StatusCode(500, "An error occurred while retrieving tasks");
        }
    }
}
