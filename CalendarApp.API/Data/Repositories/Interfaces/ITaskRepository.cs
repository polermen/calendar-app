using CalendarApp.API.Models.Entities;

namespace CalendarApp.API.Data.Repositories.Interfaces;

public interface ITaskRepository
{
    Task<TaskEntity?> GetByIdAsync(int taskId, int userId);
    Task<List<TaskEntity>> GetAllAsync(int userId);
    Task<List<TaskEntity>> GetByDateRangeAsync(int userId, DateTime startDate, DateTime endDate);
    Task<List<TaskEntity>> GetByScopeAsync(int userId, string scope, DateTime? date = null);
    Task<TaskEntity> CreateAsync(TaskEntity task);
    Task<TaskEntity> UpdateAsync(TaskEntity task);
    Task<bool> DeleteAsync(int taskId, int userId);
}
