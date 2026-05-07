namespace Ocow.Redis.Options;

/// <summary>
/// Redis 配置实体，用于绑定连接地址、键前缀和默认过期时间。
/// </summary>
public class RedisOption
{
    /// <summary>
    /// Redis 连接字符串。
    /// </summary>
    public string Configuration { get; init; } = "localhost:6379,abortConnect=false";

    /// <summary>
    /// Redis 业务键前缀，用于隔离不同服务的缓存键。
    /// </summary>
    public string KeyPrefix { get; init; } = "ocow";

    /// <summary>
    /// 默认缓存过期秒数。
    /// </summary>
    public int DefaultExpireSeconds { get; init; } = 300;

    /// <summary>
    /// 默认 Redis 数据库编号，为空时使用连接字符串中的配置。
    /// </summary>
    public int? DefaultDatabase { get; init; }
}
