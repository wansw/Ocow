using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Ocow.HealthChecks.Options;

namespace Ocow.HealthChecks.Extensions;

/// <summary>
/// 健康检查服务注册扩展。
/// </summary>
public static class HealthChecksServiceCollectionExtensions
{
    /// <summary>
    /// 注册 Ocow 健康检查服务。
    /// </summary>
    public static IServiceCollection AddOcowHealthChecks(this IServiceCollection services, IConfiguration configuration, string serviceName)
    {
        return services.AddOcowHealthChecks(configuration, serviceName, null);
    }

    /// <summary>
    /// 注册 Ocow 健康检查服务，并允许业务服务追加 ready 依赖检查。
    /// </summary>
    public static IServiceCollection AddOcowHealthChecks(
        this IServiceCollection services,
        IConfiguration configuration,
        string serviceName,
        Action<IHealthChecksBuilder>? configureChecks)
    {
        services.Configure<HealthCheckOption>(configuration.GetSection("HealthChecks"));
        services.PostConfigure<HealthCheckOption>(option =>
        {
            if (string.IsNullOrWhiteSpace(option.ServiceName) || option.ServiceName == "Ocow.Api")
            {
                option.ServiceName = serviceName;
            }
        });

        var builder = services.AddHealthChecks();
        configureChecks?.Invoke(builder);

        return services;
    }
}
