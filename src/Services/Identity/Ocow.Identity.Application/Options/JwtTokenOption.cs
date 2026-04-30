namespace Ocow.Identity.Application.Options;

/// <summary>
/// JWT 配置实体，用于绑定签发方、受众、密钥和有效期。
/// </summary>
public class JwtTokenOption
{
    public string Issuer { get; init; } = "Ocow.Identity";

    public string Audience { get; init; } = "Ocow.Clients";

    public string Secret { get; init; } = "PleaseChangeThisIdentityJwtSecretForProduction";

    public int AccessTokenMinutes { get; init; } = 120;

    public int RefreshTokenDays { get; init; } = 7;
}
