using System.Text.Json;
using StackExchange.Redis;

namespace LibrosAPI.Caching;

public sealed class RedisCache(IConnectionMultiplexer redis) : IRedisCache
{
    private readonly IDatabase _database = redis.GetDatabase();

    public async Task<T?> GetAsync<T>(string key)
    {
        var value = await _database.StringGetAsync(key);
        return value.IsNullOrEmpty ? default : JsonSerializer.Deserialize<T>(value.ToString());
    }

    public Task SetAsync<T>(string key, T value, TimeSpan expiration) =>
        _database.StringSetAsync(key, JsonSerializer.Serialize(value), expiration);

    public Task RemoveAsync(params string[] keys) =>
        _database.KeyDeleteAsync(keys.Select(key => (RedisKey)key).ToArray());
}
