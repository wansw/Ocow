namespace Ocow.HealthChecks.Options;

/// <summary>
/// PostgreSQL 健康检查配置项，用于读取数据库连接字符串。
/// </summary>
public class PostgreSqlHealthCheckOption
{
    /// <summary>
    /// PostgreSQL 连接字符串。
    /// </summary>
    public string ConnectionString { get; init; } = string.Empty;
}
