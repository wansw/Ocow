using Microsoft.AspNetCore.Http;
using Ocow.Gateway.Options;

namespace Ocow.Gateway.Middleware;

/// <summary>
/// 网关路由授权器接口，用于在 Ocelot 转发前执行粗粒度认证和授权策略。
/// </summary>
public interface IGatewayRouteAuthorizer
{
    /// <summary>
    /// 根据路由策略认证并授权当前请求。
    /// </summary>
    Task<GatewayRouteAuthorizationResult> AuthorizeAsync(
        HttpContext context,
        GatewayRoutePolicyOption routePolicy,
        CancellationToken cancellationToken = default);
}
