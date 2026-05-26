namespace Ocow.Auth.Options;

/// <summary>
/// 网关转发用户 JWT 配置，用于微服务验证网关签发的内部用户身份。
/// </summary>
public class GatewayForwardedJwtOption
{
    /// <summary>
    /// 是否启用网关转发用户 JWT 校验模式。
    /// </summary>
    public bool Enabled { get; init; }

    /// <summary>
    /// 是否允许直接使用 Identity 签发的原始用户 JWT，建议只在本地开发 Swagger 调试时开启。
    /// </summary>
    public bool AllowDirectIdentityJwt { get; init; }

    /// <summary>
    /// 网关转发用户 JWT 签发方。
    /// </summary>
    public string Issuer { get; init; } = "Ocow.Gateway";

    /// <summary>
    /// 网关转发用户 JWT 接收方。
    /// </summary>
    public string Audience { get; init; } = "Ocow.Microservices";

    /// <summary>
    /// 网关转发用户 JWT 签名密钥。
    /// </summary>
    public string Secret { get; init; } = "PleaseChangeThisGatewayForwardedJwtSecret";

    /// <summary>
    /// 网关转发用户 JWT 有效分钟数。
    /// </summary>
    public int TokenExpireMinutes { get; init; } = 5;

    /// <summary>
    /// 网关服务名称。
    /// </summary>
    public string GatewayName { get; init; } = "Ocow.Gateway";
}
