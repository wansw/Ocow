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

        switch (option.Provider)
        {
            case DatabaseProviderEnum.PostgreSql:
                return builder.UseNpgsql(option.ConnectionString, provider =>
                {
                    if (!string.IsNullOrWhiteSpace(option.MigrationsAssembly))
                    {
                        provider.MigrationsAssembly(option.MigrationsAssembly);
                    }
                });

            case DatabaseProviderEnum.MySql:
                return builder.UseMySql(option.ConnectionString, ServerVersion.AutoDetect(option.ConnectionString), provider =>
                {
                    if (!string.IsNullOrWhiteSpace(option.MigrationsAssembly))
                    {
                        provider.MigrationsAssembly(option.MigrationsAssembly);
                    }
                });

            case DatabaseProviderEnum.SqlServer:
                return builder.UseSqlServer(option.ConnectionString, provider =>
                {
                    if (!string.IsNullOrWhiteSpace(option.MigrationsAssembly))
                    {
                        provider.MigrationsAssembly(option.MigrationsAssembly);
                    }
                });

            default:
                throw new NotSupportedException($"不支持的数据库 Provider：{option.Provider}");
        }
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
