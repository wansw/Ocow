namespace Ocow.Identity.Application.Options;

/// <summary>
/// JWT 配置实体，用于绑定签发方、受众、密钥和有效期。/// </summary>
public class JwtTokenOption
{
    /// <summary>
    /// JWT 签发方。    /// </summary>
    public string Issuer { get; init; } = "Ocow.Identity";

    /// <summary>
    /// JWT 接收方。    /// </summary>
    public string Audience { get; init; } = "Ocow.Clients";

    /// <summary>
    /// JWT 签名密钥。    /// </summary>
    public string Secret { get; init; } = "PleaseChangeThisIdentityJwtSecretForProduction";

    /// <summary>
    /// 访问 Token 有效分钟数。    /// </summary>
    public int AccessTokenMinutes { get; init; } = 120;

    /// <summary>
    /// 刷新 Token 有效天数。    /// </summary>
    public int RefreshTokenDays { get; init; } = 7;
}
