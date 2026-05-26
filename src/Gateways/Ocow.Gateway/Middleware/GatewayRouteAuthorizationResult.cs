namespace Ocow.Gateway.Middleware;

/// <summary>
/// 网关路由授权结果，用于区分未认证和已认证但无权限两类拒绝原因。
/// </summary>
public sealed class GatewayRouteAuthorizationResult
{
    private GatewayRouteAuthorizationResult(bool isAuthenticated, bool isAuthorized)
    {
        IsAuthenticated = isAuthenticated;
        IsAuthorized = isAuthorized;
    }

    /// <summary>
    /// 当前请求是否已经通过认证。
    /// </summary>
    public bool IsAuthenticated { get; }

    /// <summary>
    /// 当前请求是否通过授权策略。
    /// </summary>
    public bool IsAuthorized { get; }

    /// <summary>
    /// 创建认证和授权都通过的结果。
    /// </summary>
    public static GatewayRouteAuthorizationResult Authorized()
    {
        return new GatewayRouteAuthorizationResult(true, true);
    }

    /// <summary>
    /// 创建未认证的结果。
    /// </summary>
    public static GatewayRouteAuthorizationResult Unauthenticated()
    {
        return new GatewayRouteAuthorizationResult(false, false);
    }

    /// <summary>
    /// 创建已认证但未授权的结果。
    /// </summary>
    public static GatewayRouteAuthorizationResult Forbidden()
    {
        return new GatewayRouteAuthorizationResult(true, false);
    }
}
