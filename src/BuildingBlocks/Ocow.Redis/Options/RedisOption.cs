namespace Ocow.Redis.Options;

/// <summary>
/// Redis 配置实体，用于绑定连接地址和默认过期时间。/// </summary>
public class RedisOption
{
    public string Configuration { get; init; } = "localhost:6379";

    public int DefaultExpireSeconds { get; init; } = 300;
}
