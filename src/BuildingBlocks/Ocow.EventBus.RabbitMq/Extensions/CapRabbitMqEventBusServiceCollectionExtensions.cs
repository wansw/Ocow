extern alias CapMySql;
extern alias CapPostgreSql;
extern alias CapSqlServer;

using DotNetCore.CAP;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ocow.EventBus.Abstractions.Interfaces;
using Ocow.EventBus.Abstractions.Services;
using Ocow.EventBus.RabbitMq.Interfaces;
using Ocow.EventBus.RabbitMq.Options;
using Ocow.EventBus.RabbitMq.Services;

namespace Ocow.EventBus.RabbitMq.Extensions;

/// <summary>
/// CAP RabbitMQ 事件总线服务注册扩展。
/// </summary>
public static class CapRabbitMqEventBusServiceCollectionExtensions
{
    /// <summary>
    /// 注册 Ocow CAP RabbitMQ 事件总线。
    /// </summary>
    public static IServiceCollection AddOcowCapRabbitMqEventBus(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName = "EventBus")
    {
        var section = configuration.GetSection(sectionName);
        var options = section.Get<CapRabbitMqEventBusOptions>() ?? new CapRabbitMqEventBusOptions();
        var storageConnectionString = GetStorageConnectionString(configuration, options);

        services
            .AddOptions<CapRabbitMqEventBusOptions>()
            .Bind(section)
            .Validate(ValidateOptions, "EventBus 配置无效。")
            .ValidateOnStart();

        services.AddSingleton<IIntegrationEventNameProvider, DefaultIntegrationEventNameProvider>();
        services.AddScoped<IEventBus, CapRabbitMqEventBus>();
        services.AddScoped(typeof(ICapTransactionalExecutor<>), typeof(CapTransactionalExecutor<>));

        services.AddCap(cap =>
        {
            cap.DefaultGroupName = options.DefaultGroupName;
            cap.FailedRetryCount = options.FailedRetryCount;
            cap.FailedRetryInterval = options.FailedRetryIntervalSeconds;

            ConfigureStorage(cap, options.Storage.Provider, storageConnectionString);

            cap.UseRabbitMQ(rabbitMq =>
            {
                rabbitMq.HostName = options.RabbitMq.HostName;
                rabbitMq.Port = options.RabbitMq.Port;
                rabbitMq.UserName = options.RabbitMq.UserName;
                rabbitMq.Password = options.RabbitMq.Password;
                rabbitMq.VirtualHost = options.RabbitMq.VirtualHost;
                rabbitMq.ExchangeName = options.RabbitMq.ExchangeName;
            });

            if (options.Dashboard.Enabled)
            {
                cap.UseDashboard(dashboard =>
                {
                    dashboard.PathMatch = options.Dashboard.PathMatch;
                    dashboard.AllowAnonymousExplicit = options.Dashboard.AllowAnonymousExplicit;
                    dashboard.AuthorizationPolicy = options.Dashboard.AuthorizationPolicy;
                });
            }
        });

        return services;
    }

    /// <summary>
    /// 根据配置选择 CAP 存储 Provider。
    /// </summary>
    private static void ConfigureStorage(CapOptions cap, string provider, string connectionString)
    {
        switch (provider)
        {
            case "SqlServer":
                CapSqlServer::Microsoft.Extensions.DependencyInjection.CapOptionsExtensions.UseSqlServer(cap, connectionString);
                break;
            case "PostgreSql":
            case "PostgreSQL":
            case "Postgres":
                CapPostgreSql::Microsoft.Extensions.DependencyInjection.CapOptionsExtensions.UsePostgreSql(cap, connectionString);
                break;
            case "MySql":
            case "MySQL":
                CapMySql::Microsoft.Extensions.DependencyInjection.CapOptionsExtensions.UseMySql(cap, connectionString);
                break;
            default:
                throw new NotSupportedException($"不支持的 CAP 存储 Provider：{provider}");
        }
    }

    /// <summary>
    /// 获取 CAP 存储连接字符串。
    /// </summary>
    private static string GetStorageConnectionString(IConfiguration configuration, CapRabbitMqEventBusOptions options)
    {
        var connectionString = configuration.GetConnectionString(options.Storage.ConnectionStringName);
        if (string.IsNullOrWhiteSpace(connectionString) &&
            options.Storage.ConnectionStringName.Equals("Default", StringComparison.OrdinalIgnoreCase))
        {
            connectionString = configuration["Database:ConnectionString"];
        }

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException($"CAP 存储连接字符串 {options.Storage.ConnectionStringName} 未配置。");
        }

        return connectionString;
    }

    /// <summary>
    /// 校验 CAP RabbitMQ 事件总线配置。
    /// </summary>
    private static bool ValidateOptions(CapRabbitMqEventBusOptions options)
    {
        return !string.IsNullOrWhiteSpace(options.DefaultGroupName) &&
               options.FailedRetryCount >= 0 &&
               options.FailedRetryIntervalSeconds > 0 &&
               !string.IsNullOrWhiteSpace(options.Storage.Provider) &&
               !string.IsNullOrWhiteSpace(options.Storage.ConnectionStringName) &&
               !string.IsNullOrWhiteSpace(options.RabbitMq.HostName) &&
               options.RabbitMq.Port > 0 &&
               !string.IsNullOrWhiteSpace(options.RabbitMq.UserName) &&
               !string.IsNullOrWhiteSpace(options.RabbitMq.Password) &&
               !string.IsNullOrWhiteSpace(options.RabbitMq.VirtualHost) &&
               !string.IsNullOrWhiteSpace(options.RabbitMq.ExchangeName) &&
               (!options.Dashboard.Enabled || !string.IsNullOrWhiteSpace(options.Dashboard.PathMatch));
    }
}
