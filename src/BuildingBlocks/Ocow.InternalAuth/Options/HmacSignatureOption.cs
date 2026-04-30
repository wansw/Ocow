namespace Ocow.InternalAuth.Options;

/// <summary>
/// HMAC 签名配置，用于内部高风险接口签名校验。
/// </summary>
public class HmacSignatureOption
{
    public string Secret { get; init; } = "PleaseChangeThisInternalHmacSecret";

    public int TimestampToleranceSeconds { get; init; } = 300;
}
