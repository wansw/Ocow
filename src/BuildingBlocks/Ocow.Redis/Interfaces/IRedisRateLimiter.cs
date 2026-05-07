using Ocow.Redis.Models;

namespace Ocow.Redis.Interfaces;

/// <summary>
/// Redis 限流服务接口，用于固定窗口访问频率控制。
/// </summary>
public interface IRedisRateLimiter
{
    /// <summary>
    /// 尝试占用一次固定窗口限流额度。
    /// </summary>
    Task<RedisRateLimitResult> TryAcquireAsync(string key, long limit, TimeSpan window, CancellationToken cancellationToken = default);
}
