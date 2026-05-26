namespace Ocow.Gateway.Options;

/// <summary>
/// 网关限流配置，用于控制入口请求的固定窗口访问频率。
/// </summary>
public class GatewayRateLimitOption
{
    /// <summary>
    /// 是否启用网关限流。
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// 固定窗口内允许的最大请求数。
    /// </summary>
    public long PermitLimit { get; init; } = 120;

    /// <summary>
    /// 固定窗口秒数。
    /// </summary>
    public int WindowSeconds { get; init; } = 60;
}
