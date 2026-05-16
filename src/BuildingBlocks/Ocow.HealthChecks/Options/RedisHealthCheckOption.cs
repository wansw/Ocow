namespace Ocow.HealthChecks.Options;

/// <summary>
/// Redis 健康检查配置项，用于读取 Redis 连接地址和默认库。
/// </summary>
public class RedisHealthCheckOption
{
    /// <summary>
    /// Redis 连接字符串。
    /// </summary>
    public string Configuration { get; init; } = string.Empty;

    /// <summary>
    /// 默认 Redis 数据库编号。
    /// </summary>
    public int? DefaultDatabase { get; init; }
}
