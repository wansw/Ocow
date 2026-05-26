namespace Ocow.Gateway.Options;

/// <summary>
/// 网关路由粗粒度授权配置，用于在转发前声明路径前缀需要的认证方案和授权策略。
/// </summary>
public class GatewayRoutePolicyOption
{
    /// <summary>
    /// 需要匹配的上游路径前缀。
    /// </summary>
    public string PathPrefix { get; init; } = "/";

    /// <summary>
    /// 是否允许匿名访问。
    /// </summary>
    public bool AllowAnonymous { get; init; }

    /// <summary>
    /// ASP.NET Core 认证方案名称。
    /// </summary>
    public string? AuthenticationScheme { get; init; }

    /// <summary>
    /// ASP.NET Core 授权策略名称。
    /// </summary>
    public string? AuthorizationPolicy { get; init; }
}
