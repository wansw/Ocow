using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Ocow.InternalAuth.Interfaces;
using Ocow.InternalAuth.Options;

namespace Ocow.InternalAuth.Services;

/// <summary>
/// HMAC 签名服务实现，用于内部接口防篡改校验。
/// </summary>
public class HmacSignatureService : IHmacSignatureService
{
    private readonly HmacSignatureOption _option;

    public HmacSignatureService(IOptions<HmacSignatureOption> option)
    {
        _option = option.Value;
    }

    /// <summary>
    /// 生成内部服务请求签名。
    /// </summary>
    public string Generate(string serviceName, string method, string path, string timestamp, string nonce, string bodyHash)
    {
        var payload = string.Join('\n', serviceName, method.ToUpperInvariant(), path, timestamp, nonce, bodyHash);
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_option.Secret));
        return Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(payload)));
    }

    /// <summary>
    /// 校验内部服务请求签名。
    /// </summary>
    public bool Validate(string serviceName, string method, string path, string timestamp, string nonce, string bodyHash, string signature)
    {
        if (!DateTimeOffset.TryParse(timestamp, out var requestTime))
        {
            return false;
        }

        var seconds = Math.Abs((DateTimeOffset.UtcNow - requestTime.ToUniversalTime()).TotalSeconds);
        if (seconds > _option.TimestampToleranceSeconds)
        {
            return false;
        }

        var expected = Generate(serviceName, method, path, timestamp, nonce, bodyHash);
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(expected),
            Encoding.UTF8.GetBytes(signature.ToUpperInvariant()));
    }
}
