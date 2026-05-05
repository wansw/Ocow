using Ocow.Redis.Interfaces;
using StackExchange.Redis;

namespace Ocow.Redis.Services;

/// <summary>
/// Redis 缓存服务实现，用于操作字符串缓存。/// </summary>
public class RedisCacheService : IRedisCacheService
{
    private readonly IConnectionMultiplexer _connectionMultiplexer;

    public RedisCacheService(IConnectionMultiplexer connectionMultiplexer)
    {
        _connectionMultiplexer = connectionMultiplexer;
    }

    /// <summary>
    /// 写入字符串缓存。    /// </summary>
    public async Task SetStringAsync(string key, string value, TimeSpan? expire = null, CancellationToken cancellationToken = default)
    {
        await _connectionMultiplexer.GetDatabase().StringSetAsync(key, value, expire, When.Always);
    }

    /// <summary>
    /// 读取字符串缓存。    /// </summary>
    public async Task<string?> GetStringAsync(string key, CancellationToken cancellationToken = default)
    {
        var value = await _connectionMultiplexer.GetDatabase().StringGetAsync(key);
        return value.HasValue ? value.ToString() : null;
    }

    /// <summary>
    /// 删除指定缓存。    /// </summary>
    public async Task<bool> RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        return await _connectionMultiplexer.GetDatabase().KeyDeleteAsync(key);
    }
}
