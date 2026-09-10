namespace ProductosAPI.Caching;

public sealed class NoOpRedisCache : IRedisCache
{
    public Task<T?> GetAsync<T>(string key) => Task.FromResult<T?>(default);
    public Task SetAsync<T>(string key, T value, TimeSpan expiration) => Task.CompletedTask;
    public Task RemoveAsync(params string[] keys) => Task.CompletedTask;
}
