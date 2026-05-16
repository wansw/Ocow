using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
        services.Configure<HealthCheckOption>(configuration.GetSection("HealthChecks"));
        services.PostConfigure<HealthCheckOption>(option =>
        {
            if (string.IsNullOrWhiteSpace(option.ServiceName) || option.ServiceName == "Ocow.Api")
            {
                option.ServiceName = serviceName;
            }
        });

        services.AddHealthChecks();
        return services;
    }
}
