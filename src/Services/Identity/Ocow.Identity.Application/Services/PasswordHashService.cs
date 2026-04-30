using System.Security.Cryptography;
using System.Text;

namespace Ocow.Identity.Application.Services;

/// <summary>
/// 密码摘要服务，用于 MVP 阶段生成和校验密码哈希。
/// </summary>
public static class PasswordHashService
{
    /// <summary>
    /// 生成密码 SHA256 摘要。
    /// </summary>
    public static string Hash(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes);
    }

    /// <summary>
    /// 校验明文密码和密码摘要是否匹配。
    /// </summary>
    public static bool Verify(string password, string passwordHash)
    {
        return string.Equals(Hash(password), passwordHash, StringComparison.OrdinalIgnoreCase);
    }
}
