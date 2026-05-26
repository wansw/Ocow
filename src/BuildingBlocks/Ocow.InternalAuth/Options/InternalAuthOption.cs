namespace Ocow.InternalAuth.Options;

/// <summary>
/// 内部服务认证配置实体，用于绑。Service JWT 参数。/// </summary>
public class InternalAuthOption
{
    /// <summary>
    /// 内部服务 Token 签发方。
    /// </summary>
    public string Issuer { get; init; } = "Ocow.InternalAuth";

    /// <summary>
    /// 内部服务 Token 接收方。
    /// </summary>
    public string Audience { get; init; } = "Ocow.InternalServices";

    /// <summary>
    /// 内部服务 Token 签名密钥。
    /// </summary>
    public string Secret { get; init; } = "PleaseChangeThisInternalServiceJwtSecret";

    /// <summary>
    /// 当前服务名称，用于写入服务 Token 和 HMAC 请求头。
    /// </summary>
    public string ServiceName { get; init; } = "Ocow.Service";

    /// <summary>
    /// 内部服务 Token 过期分钟数。
    /// </summary>
    public int TokenExpireMinutes { get; init; } = 10;
}
