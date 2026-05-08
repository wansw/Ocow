using System.Security.Cryptography;
using System.Text;

namespace Ocow.InternalAuth.Constants;

/// <summary>
/// Admin Token 会话 Redis 键生成器，用于统一 Identity 写入和各服务认证校验的键名。
/// </summary>
public static class AdminTokenSessionRedisKeys
{
    private const string AdminSessionKeyPrefix = "identity:admin:session";
    private const string AdminRefreshKeyPrefix = "identity:admin:refresh";
    private const string AdminBlacklistKeyPrefix = "identity:admin:blacklist";

    /// <summary>
    /// 生成 Admin 会话缓存键。
    /// </summary>
    public static string GetSessionKey(Guid sessionId)
    {
        return $"{AdminSessionKeyPrefix}:{sessionId:N}";
    }

    /// <summary>
    /// 生成 Admin 刷新 Token 缓存键。
    /// </summary>
    public static string GetRefreshKey(string refreshToken)
    {
        return $"{AdminRefreshKeyPrefix}:{HashToken(refreshToken)}";
    }

    /// <summary>
    /// 生成 Admin JWT 黑名单缓存键。
    /// </summary>
    public static string GetBlacklistKey(string jwtId)
    {
        return $"{AdminBlacklistKeyPrefix}:{jwtId}";
    }

    /// <summary>
    /// 对刷新 Token 做哈希，避免把明文 Token 放入 Redis 键名。
    /// </summary>
    private static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
