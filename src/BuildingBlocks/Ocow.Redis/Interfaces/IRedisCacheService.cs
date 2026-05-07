namespace Ocow.Redis.Interfaces;

/// <summary>
/// Redis 缓存服务接口，用于封装缓存读写、删除和过期时间控制。
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
    /// 写入对象缓存，对象会序列化为 JSON。
    /// </summary>
    Task SetAsync<T>(string key, T value, TimeSpan? expire = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 读取对象缓存，并从 JSON 反序列化为指定类型。
    /// </summary>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// 判断指定缓存是否存在。
    /// </summary>
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// 设置指定缓存的过期时间。
    /// </summary>
    Task<bool> ExpireAsync(string key, TimeSpan expire, CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除指定缓存。
    /// </summary>
    Task<bool> RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// 按前缀批量删除缓存。
    /// </summary>
    Task<long> RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default);
}
