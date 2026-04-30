namespace Ocow.InternalAuth.Options;

/// <summary>
/// 内部服务认证配置实体，用于绑定 Service JWT 参数。
/// </summary>
public class InternalAuthOption
{
    public string Issuer { get; init; } = "Ocow.InternalAuth";

    public string Audience { get; init; } = "Ocow.InternalServices";

    public string Secret { get; init; } = "PleaseChangeThisInternalServiceJwtSecret";
}
