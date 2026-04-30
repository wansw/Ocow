using Microsoft.EntityFrameworkCore;
using Ocow.EntityFrameworkCore.Interceptors;
using Ocow.EntityFrameworkCore.Options;

namespace Ocow.EntityFrameworkCore.Extensions;

/// <summary>
/// DbContextOptionsBuilder 扩展，用于按配置选择 EF Core 数据库 Provider。
/// </summary>
public static class DbContextOptionsBuilderExtensions
{
    /// <summary>
    /// 根据数据库配置选择 PostgreSQL、MySQL 或 SQL Server Provider。
    /// </summary>
    public static DbContextOptionsBuilder UseOcowDatabase(this DbContextOptionsBuilder builder, DatabaseOption option)
    {
        if (string.IsNullOrWhiteSpace(option.ConnectionString))
        {
            throw new InvalidOperationException("数据库连接字符串不能为空。");
        }

        return option.Provider switch
        {
            DatabaseProviderEnum.PostgreSql => builder.UseNpgsql(option.ConnectionString),
            DatabaseProviderEnum.MySql => builder.UseMySql(option.ConnectionString, ServerVersion.AutoDetect(option.ConnectionString)),
            DatabaseProviderEnum.SqlServer => builder.UseSqlServer(option.ConnectionString),
            _ => throw new NotSupportedException($"不支持的数据库 Provider：{option.Provider}")
        };
    }

    /// <summary>
    /// 注册 Ocow 默认 EF Core 保存拦截器。
    /// </summary>
    public static DbContextOptionsBuilder AddOcowSaveChangesInterceptors(this DbContextOptionsBuilder builder)
    {
        return builder.AddInterceptors(
            new AuditableEntitySaveChangesInterceptor(),
            new SoftDeleteSaveChangesInterceptor());
    }
}
