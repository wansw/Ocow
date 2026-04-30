namespace Ocow.EntityFrameworkCore.Options;

/// <summary>
/// 数据库配置实体，用于绑定 Provider 和连接字符串。
/// </summary>
public class DatabaseOption
{
    public DatabaseProviderEnum Provider { get; init; } = DatabaseProviderEnum.PostgreSql;

    public string ConnectionString { get; init; } = string.Empty;
}
