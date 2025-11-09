using Location404.Data.Application.Common.Interfaces;

namespace Location404.Data.Infrastructure.Cache;

/// <summary>
/// Null Object pattern implementation of ICacheService.
/// Used when Redis/Dragonfly is not configured (e.g., local development without infrastructure).
/// All operations are no-ops.
/// </summary>
public class NullCacheService : ICacheService
{
    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        return Task.FromResult<T?>(null);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan expiration, CancellationToken cancellationToken = default) where T : class
    {
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
