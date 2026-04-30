namespace Ocow.Identity.Infrastructure.Options;

/// <summary>
/// 身份服务 PostgreSQL 配置实体，用于绑定数据库连接字符串。
/// </summary>
public class IdentityPostgreSqlOption
{
    public string ConnectionString { get; init; } = "Host=localhost;Port=5432;Database=ocow_identity;Username=postgres;Password=postgres123";
}
