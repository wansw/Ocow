using Microsoft.Extensions.Options;
using Ocow.Redis.Interfaces;
using Ocow.Redis.Models;
using Ocow.Redis.Options;
using StackExchange.Redis;

namespace Ocow.Redis.Services;

/// <summary>
/// Redis 固定窗口限流服务实现。
/// </summary>
public class RedisRateLimiter : IRedisRateLimiter
{
    private readonly IConnectionMultiplexer _connectionMultiplexer;
    private readonly RedisOption _option;

    /// <summary>
    /// 创建 Redis 固定窗口限流服务。
    /// </summary>
    public RedisRateLimiter(IConnectionMultiplexer connectionMultiplexer, IOptions<RedisOption> option)
    {
        _connectionMultiplexer = connectionMultiplexer;
        _option = option.Value;
    }

    /// <summary>
    /// 尝试占用一次固定窗口限流额度。
    /// </summary>
    public async Task<RedisRateLimitResult> TryAcquireAsync(string key, long limit, TimeSpan window, CancellationToken cancellationToken = default)
    {
        if (limit <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(limit), "限流次数必须大于 0。");
        }

        if (window <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(window), "限流窗口必须大于 0。");
        }

        var database = GetDatabase();
        var redisKey = BuildKey(key);
        var used = await database.StringIncrementAsync(redisKey);
        if (used == 1)
        {
            await database.KeyExpireAsync(redisKey, window);
        }

        var ttl = await database.KeyTimeToLiveAsync(redisKey);
        var allowed = used <= limit;
        return new RedisRateLimitResult
        {
            Allowed = allowed,
            Limit = limit,
            Used = used,
            Remaining = Math.Max(0, limit - used),
            RetryAfter = allowed ? null : ttl
        };
    }

    /// <summary>
    /// 获取当前配置对应的 Redis 数据库。
    /// </summary>
    private IDatabase GetDatabase()
    {
        return _connectionMultiplexer.GetDatabase(_option.DefaultDatabase ?? -1);
    }

    /// <summary>
    /// 根据服务前缀生成最终 Redis 限流键。
    /// </summary>
    private string BuildKey(string key)
    {
        var prefix = _option.KeyPrefix.Trim(':');
        var normalizedKey = key.TrimStart(':');
        return string.IsNullOrWhiteSpace(prefix) ? normalizedKey : $"{prefix}:{normalizedKey}";
    }
}
