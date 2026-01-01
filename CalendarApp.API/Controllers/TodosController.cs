using CalendarApp.API.Data.Repositories.Interfaces;
using CalendarApp.API.Models.DTOs.Todo;
using CalendarApp.API.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CalendarApp.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TodosController : ControllerBase
{
    private readonly ITodoRepository _todoRepository;
    private readonly ICalendarShareRepository _shareRepository;
    private readonly ILogger<TodosController> _logger;

    public TodosController(
        ITodoRepository todoRepository,
        ICalendarShareRepository shareRepository,
        ILogger<TodosController> logger)
    {
        _todoRepository = todoRepository;
        _shareRepository = shareRepository;
        _logger = logger;
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.Parse(userIdClaim!);
    }

    [HttpGet("lists")]
    public async Task<ActionResult<IEnumerable<TodoListDto>>> GetAllTodoLists()
    {
        try
        {
            var userId = GetUserId();
            var todoLists = await _todoRepository.GetAllAsync(userId);

            var listDtos = todoLists.Select(list => new TodoListDto
            {
                TodoListId = list.TodoListId,
                Title = list.Title,
                Scope = list.Scope,
                ListDate = list.ListDate,
                CreatedAt = list.CreatedAt,
                UpdatedAt = list.UpdatedAt,
                Items = list.Items.Select(item => new TodoItemDto
                {
                    TodoItemId = item.TodoItemId,
                    Text = item.Text,
                    IsCompleted = item.IsCompleted,
                    Priority = item.Priority,
                    OrderIndex = item.OrderIndex,
                    CreatedAt = item.CreatedAt,
                    UpdatedAt = item.UpdatedAt
                }).OrderBy(i => i.OrderIndex).ToList()
            });

            return Ok(listDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting todo lists");
            return StatusCode(500, "An error occurred while retrieving todo lists");
        }
    }

    [HttpGet("lists/{id}")]
    public async Task<ActionResult<TodoListDto>> GetTodoList(int id)
    {
        try
        {
            var userId = GetUserId();
            var todoList = await _todoRepository.GetByIdAsync(id, userId);

            if (todoList == null)
                return NotFound();

            var listDto = new TodoListDto
            {
                TodoListId = todoList.TodoListId,
                Title = todoList.Title,
                Scope = todoList.Scope,
                ListDate = todoList.ListDate,
                CreatedAt = todoList.CreatedAt,
                UpdatedAt = todoList.UpdatedAt,
                Items = todoList.Items.Select(item => new TodoItemDto
                {
                    TodoItemId = item.TodoItemId,
                    Text = item.Text,
                    IsCompleted = item.IsCompleted,
                    Priority = item.Priority,
                    OrderIndex = item.OrderIndex,
                    CreatedAt = item.CreatedAt,
                    UpdatedAt = item.UpdatedAt
                }).OrderBy(i => i.OrderIndex).ToList()
            };

            return Ok(listDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting todo list {ListId}", id);
            return StatusCode(500, "An error occurred while retrieving the todo list");
        }
    }

    [HttpPost("lists")]
    public async Task<ActionResult<TodoListDto>> CreateTodoList([FromBody] CreateTodoListDto listDto)
    {
        try
        {
            var userId = GetUserId();

            var todoList = new TodoList
            {
                UserId = userId,
                Title = listDto.Title,
                Scope = listDto.Scope,
                ListDate = listDto.ListDate,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                TodoItems = new List<TodoItem>()
            };

            var createdList = await _todoRepository.CreateAsync(todoList);

            var responseDto = new TodoListDto
            {
                TodoListId = createdList.TodoListId,
                Title = todoList.Title,
                Scope = todoList.Scope,
                ListDate = todoList.ListDate,
                CreatedAt = todoList.CreatedAt,
                UpdatedAt = todoList.UpdatedAt,
                Items = new List<TodoItemDto>()
            };

            return CreatedAtAction(nameof(GetTodoList), new { id = createdList.TodoListId }, responseDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating todo list");
            return StatusCode(500, "An error occurred while creating the todo list");
        }
    }

    [HttpDelete("lists/{id}")]
    public async Task<ActionResult> DeleteTodoList(int id)
    {
        try
        {
            var userId = GetUserId();
            var deleted = await _todoRepository.DeleteAsync(id, userId);

            if (!deleted)
                return NotFound();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting todo list {ListId}", id);
            return StatusCode(500, "An error occurred while deleting the todo list");
        }
    }

    [HttpPost("lists/{listId}/items")]
    public async Task<ActionResult<TodoItemDto>> AddTodoItem(int listId, [FromBody] CreateTodoItemDto itemDto)
    {
        try
        {
            var userId = GetUserId();
            var todoList = await _todoRepository.GetByIdAsync(listId, userId);

            if (todoList == null)
                return NotFound();

            var maxOrder = todoList.Items.Any() ? todoList.Items.Max(i => i.OrderIndex) : -1;

            var todoItem = new TodoItem
            {
                TodoListId = listId,
                Text = itemDto.Text,
                IsCompleted = false,
                Priority = itemDto.Priority,
                OrderIndex = maxOrder + 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createdItem = await _todoRepository.CreateItemAsync(todoItem);

            var responseDto = new TodoItemDto
            {
                TodoItemId = createdItem.TodoItemId,
                Text = todoItem.Text,
                IsCompleted = todoItem.IsCompleted,
                Priority = todoItem.Priority,
                OrderIndex = todoItem.OrderIndex,
                CreatedAt = todoItem.CreatedAt,
                UpdatedAt = todoItem.UpdatedAt
            };

            return Ok(responseDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding todo item to list {ListId}", listId);
            return StatusCode(500, "An error occurred while adding the todo item");
        }
    }

    [HttpPatch("items/{id}/toggle")]
    public async Task<ActionResult> ToggleTodoItem(int id)
    {
        try
        {
            var todoItem = await _todoRepository.GetItemByIdAsync(id);

            if (todoItem == null)
                return NotFound();

            var userId = GetUserId();
            var todoList = await _todoRepository.GetByIdAsync(todoItem.TodoListId, userId);

            if (todoList == null)
                return NotFound();

            todoItem.IsCompleted = !todoItem.IsCompleted;
            await _todoRepository.UpdateItemAsync(todoItem);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling todo item {ItemId}", id);
            return StatusCode(500, "An error occurred while toggling the todo item");
        }
    }

    [HttpDelete("items/{id}")]
    public async Task<ActionResult> DeleteTodoItem(int id)
    {
        try
        {
            var todoItem = await _todoRepository.GetItemByIdAsync(id);

            if (todoItem == null)
                return NotFound();

            var userId = GetUserId();
            var todoList = await _todoRepository.GetByIdAsync(todoItem.TodoListId, userId);

            if (todoList == null)
                return NotFound();

            var deleted = await _todoRepository.DeleteItemAsync(id);

            if (!deleted)
                return NotFound();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting todo item {ItemId}", id);
            return StatusCode(500, "An error occurred while deleting the todo item");
        }
    }

    [HttpGet("spectate/{ownerId}/lists")]
    public async Task<ActionResult<IEnumerable<TodoListDto>>> GetSpectatedTodoLists(int ownerId)
    {
        try
        {
            var spectatorId = GetUserId();

            // Check if the current user has permission to spectate this calendar
            var shares = await _shareRepository.GetSharesBySpectatorIdAsync(spectatorId);
            var hasAccess = shares.Any(s => s.OwnerId == ownerId);

            if (!hasAccess)
                return Forbid();

            var todoLists = await _todoRepository.GetAllAsync(ownerId);

            var listDtos = todoLists.Select(list => new TodoListDto
            {
                TodoListId = list.TodoListId,
                Title = list.Title,
                Scope = list.Scope,
                ListDate = list.ListDate,
                CreatedAt = list.CreatedAt,
                UpdatedAt = list.UpdatedAt,
                Items = list.Items.Select(item => new TodoItemDto
                {
                    TodoItemId = item.TodoItemId,
                    Text = item.Text,
                    IsCompleted = item.IsCompleted,
                    Priority = item.Priority,
                    OrderIndex = item.OrderIndex,
                    CreatedAt = item.CreatedAt,
                    UpdatedAt = item.UpdatedAt
                }).OrderBy(i => i.OrderIndex).ToList()
            });

            return Ok(listDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting spectated todo lists for owner {OwnerId}", ownerId);
            return StatusCode(500, "An error occurred while retrieving todo lists");
        }
    }
}
