namespace Ocow.Redis.Models;

/// <summary>
/// Redis 限流结果，用于返回是否允许访问和当前窗口计数。
/// </summary>
public class RedisRateLimitResult
{
    /// <summary>
    /// 当前请求是否通过限流。
    /// </summary>
    public bool Allowed { get; init; }

    /// <summary>
    /// 当前窗口最大允许次数。
    /// </summary>
    public long Limit { get; init; }

    /// <summary>
    /// 当前窗口已经使用的次数。
    /// </summary>
    public long Used { get; init; }

    /// <summary>
    /// 当前窗口剩余次数。
    /// </summary>
    public long Remaining { get; init; }

    /// <summary>
    /// 当前窗口剩余时间。
    /// </summary>
    public TimeSpan? RetryAfter { get; init; }
}
