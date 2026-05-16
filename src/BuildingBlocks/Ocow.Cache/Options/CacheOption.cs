namespace Ocow.Cache.Options;

/// <summary>
/// 缓存配置实体，用于绑定缓存键前缀和默认过期时间。
/// </summary>
public class CacheOption
{
    /// <summary>
    /// 缓存键前缀，用于隔离不同服务的缓存键。
    /// </summary>
    public string KeyPrefix { get; set; } = "ocow";

    /// <summary>
    /// 默认缓存过期秒数。
    /// </summary>
    public int DefaultExpireSeconds { get; set; } = 300;
}
