using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Ocow.Gateway.Options;

namespace Ocow.Gateway.Middleware;

/// <summary>
/// 网关路由授权器，用于复用 ASP.NET Core 的 JWT 认证方案和授权策略。
/// </summary>
public sealed class GatewayRouteAuthorizer : IGatewayRouteAuthorizer
{
    private readonly IAuthenticationService _authenticationService;
    private readonly IAuthorizationService _authorizationService;

    /// <summary>
    /// 创建网关路由授权器。
    /// </summary>
    public GatewayRouteAuthorizer(IAuthenticationService authenticationService, IAuthorizationService authorizationService)
    {
        _authenticationService = authenticationService;
        _authorizationService = authorizationService;
    }

    /// <summary>
    /// 根据路由策略认证并授权当前请求。
    /// </summary>
    public async Task<GatewayRouteAuthorizationResult> AuthorizeAsync(
        HttpContext context,
        GatewayRoutePolicyOption routePolicy,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(routePolicy.AuthenticationScheme))
        {
            return GatewayRouteAuthorizationResult.Unauthenticated();
        }

        var authenticateResult = await _authenticationService.AuthenticateAsync(context, routePolicy.AuthenticationScheme);
        if (!authenticateResult.Succeeded || authenticateResult.Principal is null)
        {
            return GatewayRouteAuthorizationResult.Unauthenticated();
        }

        context.User = authenticateResult.Principal;
        if (string.IsNullOrWhiteSpace(routePolicy.AuthorizationPolicy))
        {
            return GatewayRouteAuthorizationResult.Authorized();
        }

        var authorizeResult = await _authorizationService.AuthorizeAsync(
            authenticateResult.Principal,
            resource: null,
            policyName: routePolicy.AuthorizationPolicy);

        return authorizeResult.Succeeded
            ? GatewayRouteAuthorizationResult.Authorized()
            : GatewayRouteAuthorizationResult.Forbidden();
    }
}
