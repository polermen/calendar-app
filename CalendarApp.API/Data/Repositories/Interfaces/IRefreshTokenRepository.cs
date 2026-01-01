using CalendarApp.API.Models.Entities;

namespace CalendarApp.API.Data.Repositories.Interfaces;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenAsync(string token);
    Task<RefreshToken> CreateAsync(RefreshToken refreshToken);
    Task RevokeAsync(string token);
    Task RevokeAllUserTokensAsync(int userId);
}
