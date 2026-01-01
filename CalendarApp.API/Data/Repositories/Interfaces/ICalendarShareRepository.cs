using CalendarApp.API.Models.Entities;

namespace CalendarApp.API.Data.Repositories.Interfaces;

public interface ICalendarShareRepository
{
    Task<IEnumerable<CalendarShare>> GetSharesByOwnerIdAsync(int ownerId);
    Task<IEnumerable<CalendarShare>> GetSharesBySpectatorIdAsync(int spectatorId);
    Task<IEnumerable<CalendarShare>> GetSharesBySpectatorEmailAsync(string spectatorEmail);
    Task<CalendarShare?> GetByIdAsync(int shareId);
    Task<CalendarShare> CreateAsync(CalendarShare share);
    Task<bool> DeleteAsync(int shareId, int ownerId);
    Task<bool> ShareExistsAsync(int ownerId, string spectatorEmail);
    Task UpdateSpectatorUserIdAsync(string email, int userId);
}
