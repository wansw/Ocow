namespace Ocow.Redis.Interfaces;

/// <summary>
/// Redis 分布式锁接口，用于任务幂等和并发控制。
/// </summary>
public interface IRedisLockService
{
    /// <summary>
    /// 尝试获取分布式锁。
    /// </summary>
    Task<bool> TryLockAsync(string key, string value, TimeSpan expire, CancellationToken cancellationToken = default);

    /// <summary>
    /// 释放分布式锁，只有锁值匹配时才会释放。
    /// </summary>
    Task<bool> UnlockAsync(string key, string value, CancellationToken cancellationToken = default);
}
