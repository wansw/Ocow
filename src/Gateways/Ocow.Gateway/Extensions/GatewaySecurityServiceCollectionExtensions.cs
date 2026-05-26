using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ocow.Gateway.Middleware;
using Ocow.Gateway.Options;

namespace Ocow.Gateway.Extensions;

/// <summary>
/// 网关安全服务注册扩展，用于注册入口安全配置和路由授权器。
/// </summary>
public static class GatewaySecurityServiceCollectionExtensions
{
    /// <summary>
    /// 注册网关安全配置和授权服务。
    /// </summary>
    public static IServiceCollection AddOcowGatewaySecurity(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<GatewaySecurityOption>(configuration.GetSection("GatewaySecurity"));
        services.AddSingleton<IGatewayRouteAuthorizer, GatewayRouteAuthorizer>();
        return services;
    }
}
