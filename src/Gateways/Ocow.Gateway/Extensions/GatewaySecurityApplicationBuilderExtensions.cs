using Microsoft.AspNetCore.Builder;
using Ocow.Gateway.Middleware;

namespace Ocow.Gateway.Extensions;

/// <summary>
/// 网关安全中间件扩展，用于在 Ocelot 转发前启用入口安全检查。
/// </summary>
public static class GatewaySecurityApplicationBuilderExtensions
{
    /// <summary>
    /// 启用网关入口安全中间件。
    /// </summary>
    public static IApplicationBuilder UseOcowGatewaySecurity(this IApplicationBuilder app)
    {
        return app.UseMiddleware<GatewaySecurityMiddleware>();
    }
}
