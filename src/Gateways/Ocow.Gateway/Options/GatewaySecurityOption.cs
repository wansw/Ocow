namespace Ocow.Gateway.Options;

/// <summary>
/// 网关安全配置，用于控制入口认证、IP 黑白名单、限流和路由粗粒度权限。
/// </summary>
public class GatewaySecurityOption
{
    /// <summary>
    /// 是否启用网关安全层。
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// 是否拒绝未配置粗粒度策略的路由。
    /// </summary>
    public bool DenyUnmatchedRoutes { get; init; } = true;

    /// <summary>
    /// 是否信任 X-Forwarded-For 请求头，只有网关前方存在可信反向代理时才应开启。
    /// </summary>
    public bool TrustForwardedForHeader { get; init; }

    /// <summary>
    /// 允许访问网关的 IP 白名单，留空表示不限制。
    /// </summary>
    public IReadOnlyCollection<string> IpWhitelist { get; init; } = [];

    /// <summary>
    /// 禁止访问网关的 IP 黑名单。
    /// </summary>
    public IReadOnlyCollection<string> IpBlacklist { get; init; } = [];

    /// <summary>
    /// 网关入口限流配置。
    /// </summary>
    public GatewayRateLimitOption RateLimit { get; init; } = new();

    /// <summary>
    /// 网关路由粗粒度授权配置。
    /// </summary>
    public IReadOnlyCollection<GatewayRoutePolicyOption> Routes { get; init; } = [];
}
