namespace Ocow.InternalAuth.Options;

/// <summary>
/// Identity JWT 验证配置，用于校。Customer JWT 。Admin JWT。/// </summary>
public class IdentityJwtOption
{
    public string Issuer { get; init; } = "Ocow.Identity";

    public string Audience { get; init; } = "Ocow.Clients";

    public string Secret { get; init; } = "OcowIdentityJwtSecret@2026-ChangeInProduction";
}
