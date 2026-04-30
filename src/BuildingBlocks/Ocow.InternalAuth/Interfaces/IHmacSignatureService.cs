namespace Ocow.InternalAuth.Interfaces;

/// <summary>
/// HMAC 签名服务接口，用于生成和校验内部服务请求签名。
/// </summary>
public interface IHmacSignatureService
{
    /// <summary>
    /// 生成内部服务请求签名。
    /// </summary>
    string Generate(string serviceName, string method, string path, string timestamp, string nonce, string bodyHash);

    /// <summary>
    /// 校验内部服务请求签名。
    /// </summary>
    bool Validate(string serviceName, string method, string path, string timestamp, string nonce, string bodyHash, string signature);
}
