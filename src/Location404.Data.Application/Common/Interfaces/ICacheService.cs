namespace Location404.Data.Application.Common.Interfaces;

/// <summary>
/// Distributed cache service interface for caching frequently accessed data.
/// Implementations should use Redis/Dragonfly for distributed caching across multiple replicas.
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Gets a cached value by key, deserializing from JSON.
    /// </summary>
    /// <typeparam name="T">Type to deserialize to</typeparam>
    /// <param name="key">Cache key</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Cached value or null if not found/expired</returns>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Sets a cached value with expiration, serializing to JSON.
    /// </summary>
    /// <typeparam name="T">Type to serialize</typeparam>
    /// <param name="key">Cache key</param>
    /// <param name="value">Value to cache</param>
    /// <param name="expiration">Time until expiration</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SetAsync<T>(string key, T value, TimeSpan expiration, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Removes a cached value by key.
    /// </summary>
    /// <param name="key">Cache key</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes all cached values matching a pattern (e.g., "player:stats:*").
    /// Use with caution - can be expensive on large keyspaces.
    /// </summary>
    /// <param name="pattern">Key pattern (supports Redis glob-style patterns)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default);
}
