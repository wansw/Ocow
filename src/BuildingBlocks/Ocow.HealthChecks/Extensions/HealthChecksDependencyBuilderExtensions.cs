using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Ocow.HealthChecks.Checks;
using Ocow.HealthChecks.Constants;
using Ocow.HealthChecks.Options;

namespace Ocow.HealthChecks.Extensions;

/// <summary>
/// 健康检查依赖注册扩展。
/// </summary>
public static class HealthChecksDependencyBuilderExtensions
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(5);

    /// <summary>
    /// 注册 PostgreSQL ready 健康检查。
    /// </summary>
    public static IHealthChecksBuilder AddPostgreSqlCheck(
        this IHealthChecksBuilder builder,
        IConfiguration configuration,
        string sectionName = "Database")
    {
        var option = configuration.GetSection(sectionName).Get<PostgreSqlHealthCheckOption>() ?? new PostgreSqlHealthCheckOption();
        if (string.IsNullOrWhiteSpace(option.ConnectionString))
        {
            return builder;
        }

        return AddReadyCheck(builder, "postgresql", _ => new PostgreSqlHealthCheck(option.ConnectionString));
    }

    /// <summary>
    /// 注册 Redis ready 健康检查。
    /// </summary>
    public static IHealthChecksBuilder AddRedisCheck(
        this IHealthChecksBuilder builder,
        IConfiguration configuration,
        string sectionName = "Redis")
    {
        var option = configuration.GetSection(sectionName).Get<RedisHealthCheckOption>() ?? new RedisHealthCheckOption();
        if (string.IsNullOrWhiteSpace(option.Configuration))
        {
            return builder;
        }

        return AddReadyCheck(builder, "redis", _ => new RedisHealthCheck(option));
    }

    /// <summary>
    /// 注册 RabbitMQ ready 健康检查。
    /// </summary>
    public static IHealthChecksBuilder AddRabbitMqCheck(
        this IHealthChecksBuilder builder,
        IConfiguration configuration,
        string sectionName = "RabbitMq")
    {
        var option = configuration.GetSection(sectionName).Get<RabbitMqHealthCheckOption>() ?? new RabbitMqHealthCheckOption();
        if (string.IsNullOrWhiteSpace(option.HostName))
        {
            return builder;
        }

        return AddReadyCheck(builder, "rabbitmq", _ => new RabbitMqHealthCheck(option));
    }

    /// <summary>
    /// 按统一名称、失败状态、ready 标签和超时时间注册依赖健康检查。
    /// </summary>
    private static IHealthChecksBuilder AddReadyCheck(
        IHealthChecksBuilder builder,
        string name,
        Func<IServiceProvider, IHealthCheck> factory)
    {
        builder.Add(new HealthCheckRegistration(
            name,
            factory,
            HealthStatus.Unhealthy,
            new[] { HealthCheckTags.Ready },
            DefaultTimeout));

        return builder;
    }
}
