namespace Ocow.Auth.Options;

/// <summary>
/// Identity JWT 校验配置，用于校验 Customer JWT 和 Admin JWT。
/// </summary>
public class IdentityJwtOption
{
    /// <summary>
    /// JWT 签发方。
    /// </summary>
    public string Issuer { get; init; } = "Ocow.Identity";

    /// <summary>
    /// JWT 接收方。
    /// </summary>
    public string Audience { get; init; } = "Ocow.Clients";

    /// <summary>
    /// JWT 签名密钥。
    /// </summary>
    public string Secret { get; init; } = "OcowIdentityJwtSecret@2026-ChangeInProduction";
}
