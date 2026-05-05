using Ocow.Redis.Interfaces;
using StackExchange.Redis;

namespace Ocow.Redis.Services;

/// <summary>
/// Redis 分布式锁服务实现，用于避免重复执行高风险操作。/// </summary>
public class RedisLockService : IRedisLockService
{
    private readonly IConnectionMultiplexer _connectionMultiplexer;

    public RedisLockService(IConnectionMultiplexer connectionMultiplexer)
    {
        _connectionMultiplexer = connectionMultiplexer;
    }

    /// <summary>
    /// 尝试获取分布式锁。    /// </summary>
    public async Task<bool> TryLockAsync(string key, string value, TimeSpan expire, CancellationToken cancellationToken = default)
    {
        return await _connectionMultiplexer.GetDatabase().StringSetAsync(key, value, expire, When.NotExists);
    }

    /// <summary>
    /// 释放分布式锁。    /// </summary>
    public async Task<bool> UnlockAsync(string key, string value, CancellationToken cancellationToken = default)
    {
        var database = _connectionMultiplexer.GetDatabase();
        var currentValue = await database.StringGetAsync(key);
        if (currentValue != value)
        {
            return false;
        }

        return await database.KeyDeleteAsync(key);
    }
}
