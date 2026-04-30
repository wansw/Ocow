namespace Ocow.Redis.Interfaces;

/// <summary>
/// Redis 缓存服务接口，用于封装字符串缓存读写删除。
/// </summary>
public interface IRedisCacheService
{
    /// <summary>
    /// 写入字符串缓存。
    /// </summary>
    Task SetStringAsync(string key, string value, TimeSpan? expire = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 读取字符串缓存。
    /// </summary>
    Task<string?> GetStringAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除指定缓存。
    /// </summary>
    Task<bool> RemoveAsync(string key, CancellationToken cancellationToken = default);
}
