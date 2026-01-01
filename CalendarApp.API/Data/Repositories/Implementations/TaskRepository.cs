using CalendarApp.API.Data.Repositories.Interfaces;
using CalendarApp.API.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace CalendarApp.API.Data.Repositories.Implementations;

public class TaskRepository : ITaskRepository
{
    private readonly ApplicationDbContext _context;

    public TaskRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TaskEntity?> GetByIdAsync(int taskId, int userId)
    {
        return await _context.Tasks
            .FirstOrDefaultAsync(t => t.TaskId == taskId && t.UserId == userId);
    }

    public async Task<List<TaskEntity>> GetAllAsync(int userId)
    {
        return await _context.Tasks
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<TaskEntity>> GetByDateRangeAsync(int userId, DateTime startDate, DateTime endDate)
    {
        return await _context.Tasks
            .Where(t => t.UserId == userId &&
                       ((t.TaskDate >= startDate && t.TaskDate <= endDate) ||
                        (t.StartDate >= startDate && t.StartDate <= endDate) ||
                        (t.EndDate >= startDate && t.EndDate <= endDate)))
            .OrderBy(t => t.TaskDate ?? t.StartDate)
            .ToListAsync();
    }

    public async Task<List<TaskEntity>> GetByScopeAsync(int userId, string scope, DateTime? date = null)
    {
        var query = _context.Tasks.Where(t => t.UserId == userId && t.Scope == scope);

        if (date.HasValue)
        {
            var targetDate = date.Value.Date;
            query = scope switch
            {
                "Day" => query.Where(t => t.TaskDate.HasValue && t.TaskDate.Value.Date == targetDate),
                "Week" => query.Where(t => t.TaskDate.HasValue &&
                    EF.Functions.DateDiffDay(targetDate, t.TaskDate.Value) >= 0 &&
                    EF.Functions.DateDiffDay(targetDate, t.TaskDate.Value) < 7),
                "Month" => query.Where(t => t.TaskDate.HasValue &&
                    t.TaskDate.Value.Year == targetDate.Year &&
                    t.TaskDate.Value.Month == targetDate.Month),
                "Year" => query.Where(t => t.TaskDate.HasValue &&
                    t.TaskDate.Value.Year == targetDate.Year),
                _ => query
            };
        }

        return await query.OrderBy(t => t.TaskDate ?? t.StartDate).ToListAsync();
    }

    public async Task<TaskEntity> CreateAsync(TaskEntity task)
    {
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();
        return task;
    }

    public async Task<TaskEntity> UpdateAsync(TaskEntity task)
    {
        task.UpdatedAt = DateTime.UtcNow;
        _context.Tasks.Update(task);
        await _context.SaveChangesAsync();
        return task;
    }

    public async Task<bool> DeleteAsync(int taskId, int userId)
    {
        var task = await GetByIdAsync(taskId, userId);
        if (task == null) return false;

        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();
        return true;
    }
}
