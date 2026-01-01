using CalendarApp.API.Models.Entities;

namespace CalendarApp.API.Data.Repositories.Interfaces;

public interface ITodoRepository
{
    Task<TodoList?> GetByIdAsync(int todoListId, int userId);
    Task<List<TodoList>> GetAllAsync(int userId);
    Task<List<TodoList>> GetByScopeAsync(int userId, string scope, DateTime? date = null);
    Task<TodoList> CreateAsync(TodoList todoList);
    Task<TodoList> UpdateAsync(TodoList todoList);
    Task<bool> DeleteAsync(int todoListId, int userId);

    Task<TodoItem?> GetItemByIdAsync(int todoItemId);
    Task<TodoItem> CreateItemAsync(TodoItem item);
    Task<TodoItem> UpdateItemAsync(TodoItem item);
    Task<bool> DeleteItemAsync(int todoItemId);
}
