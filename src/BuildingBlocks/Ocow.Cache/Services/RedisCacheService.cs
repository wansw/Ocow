using System.Text.Json;
using Microsoft.Extensions.Options;
using Ocow.Cache.Constants;
using Ocow.Cache.Interfaces;
using Ocow.Cache.Options;
using StackExchange.Redis;

namespace Ocow.Cache.Services;

/// <summary>
/// Redis 缓存服务实现，用于操作字符串缓存和 JSON 对象缓存。
/// </summary>
public class RedisCacheService : ICacheService
{
    private readonly IConnectionMultiplexer _connectionMultiplexer;
    private readonly CacheOption _option;

    /// <summary>
    /// 创建 Redis 缓存服务。
    /// </summary>
    public RedisCacheService(IConnectionMultiplexer connectionMultiplexer, IOptions<CacheOption> option)
    {
        _connectionMultiplexer = connectionMultiplexer;
        _option = option.Value;
    }

    /// <summary>
    /// 写入字符串缓存。
    /// </summary>
    public async Task SetStringAsync(string key, string value, TimeSpan? expire = null, CancellationToken cancellationToken = default)
    {
        await GetDatabase().StringSetAsync(BuildKey(key), value, expire ?? GetDefaultExpire(), When.Always);
    }

    /// <summary>
    /// 读取字符串缓存。
    /// </summary>
    public async Task<string?> GetStringAsync(string key, CancellationToken cancellationToken = default)
    {
        var value = await GetDatabase().StringGetAsync(BuildKey(key));
        return value.HasValue ? value.ToString() : null;
    }

    /// <summary>
    /// 写入对象缓存，对象会序列化为 JSON。
    /// </summary>
    public async Task SetAsync<T>(string key, T value, TimeSpan? expire = null, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(value);
        await SetStringAsync(key, json, expire, cancellationToken);
    }

    /// <summary>
    /// 读取对象缓存，并从 JSON 反序列化为指定类型。
    /// </summary>
    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var json = await GetStringAsync(key, cancellationToken);
        return string.IsNullOrWhiteSpace(json) ? default : JsonSerializer.Deserialize<T>(json);
    }

    /// <summary>
    /// 判断指定缓存是否存在。
    /// </summary>
    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        return await GetDatabase().KeyExistsAsync(BuildKey(key));
    }

    /// <summary>
    /// 设置指定缓存的过期时间。
    /// </summary>
    public async Task<bool> ExpireAsync(string key, TimeSpan expire, CancellationToken cancellationToken = default)
    {
        return await GetDatabase().KeyExpireAsync(BuildKey(key), expire);
    }

    /// <summary>
    /// 删除指定缓存。
    /// </summary>
    public async Task<bool> RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        return await GetDatabase().KeyDeleteAsync(BuildKey(key));
    }

    /// <summary>
    /// 按前缀批量删除缓存。
    /// </summary>
    public async Task<long> RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
    {
        var database = GetDatabase();
        var pattern = $"{BuildKey(prefix)}*";
        var keys = _connectionMultiplexer
            .GetEndPoints()
            .Select(endpoint => _connectionMultiplexer.GetServer(endpoint))
            .Where(server => server.IsConnected)
            .SelectMany(server => server.Keys(database.Database, pattern))
            .Distinct()
            .ToArray();

        return keys.Length == 0 ? 0 : await database.KeyDeleteAsync(keys);
    }

    /// <summary>
    /// 获取当前配置对应的 Redis 数据库。
    /// </summary>
    private IDatabase GetDatabase()
    {
        return _connectionMultiplexer.GetDatabase();
    }

    /// <summary>
    /// 根据服务前缀生成最终 Redis 缓存键。
    /// </summary>
    private string BuildKey(string key)
    {
        return CacheKeys.Build(_option.KeyPrefix, key);
    }

    /// <summary>
    /// 获取默认缓存过期时间。
    /// </summary>
    private TimeSpan GetDefaultExpire()
    {
        return TimeSpan.FromSeconds(Math.Max(1, _option.DefaultExpireSeconds));
    }
}
