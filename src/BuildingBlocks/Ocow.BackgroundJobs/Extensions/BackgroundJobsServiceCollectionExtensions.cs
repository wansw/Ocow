using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Ocow.BackgroundJobs.Jobs;
using Ocow.BackgroundJobs.Options;

namespace Ocow.BackgroundJobs.Extensions;

/// <summary>
/// 后台任务服务注册扩展，用于接入 Hangfire 存储、Server 和测试任务示例。
/// </summary>
public static class BackgroundJobsServiceCollectionExtensions
{
    /// <summary>
    /// 注册 Ocow 后台任务能力，默认使用 PostgreSQL 作为 Hangfire 持久化存储。
    /// </summary>
    public static IServiceCollection AddOcowBackgroundJobs(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<BackgroundJobsOption>(configuration.GetSection("BackgroundJobs"));
        services.PostConfigure<BackgroundJobsOption>(option =>
        {
            if (string.IsNullOrWhiteSpace(option.StorageConnectionString))
            {
                option.StorageConnectionString = configuration.GetConnectionString("Hangfire")
                    ?? configuration.GetSection("Database")["ConnectionString"]
                    ?? string.Empty;
            }
        });

        var backgroundJobsOption = configuration.GetSection("BackgroundJobs").Get<BackgroundJobsOption>() ?? new BackgroundJobsOption();
        if (string.IsNullOrWhiteSpace(backgroundJobsOption.StorageConnectionString))
        {
            backgroundJobsOption.StorageConnectionString = configuration.GetConnectionString("Hangfire")
                ?? configuration.GetSection("Database")["ConnectionString"]
                ?? string.Empty;
        }

        if (string.IsNullOrWhiteSpace(backgroundJobsOption.StorageConnectionString))
        {
            throw new InvalidOperationException("请配置 BackgroundJobs:StorageConnectionString、ConnectionStrings:Hangfire 或 Database:ConnectionString。");
        }

        services.AddHangfire(hangfire =>
        {
            hangfire
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UsePostgreSqlStorage(
                    storage => storage.UseNpgsqlConnection(backgroundJobsOption.StorageConnectionString),
                    new PostgreSqlStorageOptions
                    {
                        SchemaName = backgroundJobsOption.SchemaName
                    });
        });
        services.AddHangfireServer();
        services.TryAddTransient<SampleBackgroundJob>();

        return services;
    }
}
