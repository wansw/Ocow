using Microsoft.Extensions.Options;
using Ocow.Redis.Interfaces;
using Ocow.Redis.Options;
using StackExchange.Redis;

namespace Ocow.Redis.Services;

/// <summary>
/// Redis 分布式锁服务实现，用于避免重复执行高风险操作。
/// </summary>
public class RedisLockService : IRedisLockService
{
    private const string UnlockScript = """
        if redis.call('get', KEYS[1]) == ARGV[1] then
            return redis.call('del', KEYS[1])
        else
            return 0
        end
        """;

    private readonly IConnectionMultiplexer _connectionMultiplexer;
    private readonly RedisOption _option;

    /// <summary>
    /// 创建 Redis 分布式锁服务。
    /// </summary>
    public RedisLockService(IConnectionMultiplexer connectionMultiplexer, IOptions<RedisOption> option)
    {
        _connectionMultiplexer = connectionMultiplexer;
        _option = option.Value;
    }

    /// <summary>
    /// 尝试获取分布式锁。
    /// </summary>
    public async Task<bool> TryLockAsync(string key, string value, TimeSpan expire, CancellationToken cancellationToken = default)
    {
        return await GetDatabase().StringSetAsync(BuildKey(key), value, expire, When.NotExists);
    }

    /// <summary>
    /// 释放分布式锁，只有锁值匹配时才会释放。
    /// </summary>
    public async Task<bool> UnlockAsync(string key, string value, CancellationToken cancellationToken = default)
    {
        var result = await GetDatabase().ScriptEvaluateAsync(
            UnlockScript,
            new RedisKey[] { BuildKey(key) },
            new RedisValue[] { value });

        return (long)result == 1;
    }

    /// <summary>
    /// 获取当前配置对应的 Redis 数据库。
    /// </summary>
    private IDatabase GetDatabase()
    {
        return _connectionMultiplexer.GetDatabase(_option.DefaultDatabase ?? -1);
    }

    /// <summary>
    /// 根据服务前缀生成最终 Redis 锁键。
    /// </summary>
    private string BuildKey(string key)
    {
        var prefix = _option.KeyPrefix.Trim(':');
        var normalizedKey = key.TrimStart(':');
        return string.IsNullOrWhiteSpace(prefix) ? normalizedKey : $"{prefix}:{normalizedKey}";
    }
}
