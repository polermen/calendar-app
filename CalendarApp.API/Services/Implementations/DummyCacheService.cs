using CalendarApp.API.Services.Interfaces;

namespace CalendarApp.API.Services.Implementations;

/// <summary>
/// Temporary cache service that does nothing - used when Redis is not available
/// </summary>
public class DummyCacheService : ICacheService
{
    public Task<T?> GetAsync<T>(string key)
    {
        return Task.FromResult<T?>(default);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key)
    {
        return Task.CompletedTask;
    }

    public Task RemoveByPatternAsync(string pattern)
    {
        return Task.CompletedTask;
    }
}
