using CalendarApp.API.Data.Repositories.Interfaces;
using CalendarApp.API.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace CalendarApp.API.Data.Repositories.Implementations;

public class CalendarShareRepository : ICalendarShareRepository
{
    private readonly ApplicationDbContext _context;

    public CalendarShareRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CalendarShare>> GetSharesByOwnerIdAsync(int ownerId)
    {
        return await _context.CalendarShares
            .Include(s => s.SpectatorUser)
            .Where(s => s.OwnerId == ownerId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<CalendarShare>> GetSharesBySpectatorIdAsync(int spectatorId)
    {
        return await _context.CalendarShares
            .Include(s => s.Owner)
            .Where(s => s.SpectatorUserId == spectatorId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<CalendarShare>> GetSharesBySpectatorEmailAsync(string spectatorEmail)
    {
        return await _context.CalendarShares
            .Include(s => s.Owner)
            .Where(s => s.SpectatorEmail.ToLower() == spectatorEmail.ToLower())
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task<CalendarShare?> GetByIdAsync(int shareId)
    {
        return await _context.CalendarShares
            .Include(s => s.Owner)
            .Include(s => s.SpectatorUser)
            .FirstOrDefaultAsync(s => s.CalendarShareId == shareId);
    }

    public async Task<CalendarShare> CreateAsync(CalendarShare share)
    {
        _context.CalendarShares.Add(share);
        await _context.SaveChangesAsync();
        return share;
    }

    public async Task<bool> DeleteAsync(int shareId, int ownerId)
    {
        var share = await _context.CalendarShares
            .FirstOrDefaultAsync(s => s.CalendarShareId == shareId && s.OwnerId == ownerId);

        if (share == null)
            return false;

        _context.CalendarShares.Remove(share);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ShareExistsAsync(int ownerId, string spectatorEmail)
    {
        return await _context.CalendarShares
            .AnyAsync(s => s.OwnerId == ownerId && s.SpectatorEmail.ToLower() == spectatorEmail.ToLower());
    }

    public async Task UpdateSpectatorUserIdAsync(string email, int userId)
    {
        var shares = await _context.CalendarShares
            .Where(s => s.SpectatorEmail.ToLower() == email.ToLower() && s.SpectatorUserId == null)
            .ToListAsync();

        foreach (var share in shares)
        {
            share.SpectatorUserId = userId;
            share.IsAccepted = true;
        }

        await _context.SaveChangesAsync();
    }
}
