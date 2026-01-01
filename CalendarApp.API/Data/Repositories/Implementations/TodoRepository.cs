using CalendarApp.API.Data.Repositories.Interfaces;
using CalendarApp.API.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace CalendarApp.API.Data.Repositories.Implementations;

public class TodoRepository : ITodoRepository
{
    private readonly ApplicationDbContext _context;

    public TodoRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TodoList?> GetByIdAsync(int todoListId, int userId)
    {
        return await _context.TodoLists
            .Include(tl => tl.TodoItems.OrderBy(i => i.OrderIndex))
            .FirstOrDefaultAsync(tl => tl.TodoListId == todoListId && tl.UserId == userId);
    }

    public async Task<List<TodoList>> GetAllAsync(int userId)
    {
        return await _context.TodoLists
            .Include(tl => tl.TodoItems.OrderBy(i => i.OrderIndex))
            .Where(tl => tl.UserId == userId)
            .OrderByDescending(tl => tl.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<TodoList>> GetByScopeAsync(int userId, string scope, DateTime? date = null)
    {
        var query = _context.TodoLists
            .Include(tl => tl.TodoItems.OrderBy(i => i.OrderIndex))
            .Where(tl => tl.UserId == userId && tl.Scope == scope);

        if (date.HasValue)
        {
            var targetDate = date.Value.Date;
            query = scope switch
            {
                "Day" => query.Where(tl => tl.ListDate.HasValue && tl.ListDate.Value.Date == targetDate),
                "Week" => query.Where(tl => tl.ListDate.HasValue &&
                    EF.Functions.DateDiffDay(targetDate, tl.ListDate.Value) >= 0 &&
                    EF.Functions.DateDiffDay(targetDate, tl.ListDate.Value) < 7),
                "Month" => query.Where(tl => tl.ListDate.HasValue &&
                    tl.ListDate.Value.Year == targetDate.Year &&
                    tl.ListDate.Value.Month == targetDate.Month),
                "Year" => query.Where(tl => tl.ListDate.HasValue &&
                    tl.ListDate.Value.Year == targetDate.Year),
                _ => query
            };
        }

        return await query.OrderBy(tl => tl.ListDate).ToListAsync();
    }

    public async Task<TodoList> CreateAsync(TodoList todoList)
    {
        _context.TodoLists.Add(todoList);
        await _context.SaveChangesAsync();
        return todoList;
    }

    public async Task<TodoList> UpdateAsync(TodoList todoList)
    {
        todoList.UpdatedAt = DateTime.UtcNow;
        _context.TodoLists.Update(todoList);
        await _context.SaveChangesAsync();
        return todoList;
    }

    public async Task<bool> DeleteAsync(int todoListId, int userId)
    {
        var todoList = await GetByIdAsync(todoListId, userId);
        if (todoList == null) return false;

        _context.TodoLists.Remove(todoList);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<TodoItem?> GetItemByIdAsync(int todoItemId)
    {
        return await _context.TodoItems
            .Include(ti => ti.TodoList)
            .FirstOrDefaultAsync(ti => ti.TodoItemId == todoItemId);
    }

    public async Task<TodoItem> CreateItemAsync(TodoItem item)
    {
        _context.TodoItems.Add(item);
        await _context.SaveChangesAsync();
        return item;
    }

    public async Task<TodoItem> UpdateItemAsync(TodoItem item)
    {
        item.UpdatedAt = DateTime.UtcNow;
        _context.TodoItems.Update(item);
        await _context.SaveChangesAsync();
        return item;
    }

    public async Task<bool> DeleteItemAsync(int todoItemId)
    {
        var item = await GetItemByIdAsync(todoItemId);
        if (item == null) return false;

        _context.TodoItems.Remove(item);
        await _context.SaveChangesAsync();
        return true;
    }
}
