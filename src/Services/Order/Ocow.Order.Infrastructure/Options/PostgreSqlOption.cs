namespace Ocow.Order.Infrastructure.Options;

/// <summary>
/// PostgreSQL 配置实体，用于绑定订单服务数据库连接字符串。
/// </summary>
public class PostgreSqlOption
{
    public string ConnectionString { get; init; } = "Host=localhost;Port=5432;Database=ocow_order;Username=postgres;Password=postgres123";
}
