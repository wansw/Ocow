using Ocow.Identity.Application.Interfaces;
using Ocow.Identity.Application.Models;
using Ocow.InternalAuth.Constants;
using Ocow.InternalAuth.Models;
using Ocow.Redis.Interfaces;

namespace Ocow.Identity.Application.Services;

/// <summary>
/// Token 会话服务实现，用 Redis 管理 Admin JWT 会话、刷新 Token 缓存和 JWT 黑名单。
/// </summary>
public class RedisTokenSessionService : IRedisTokenSessionService
{
    private readonly IRedisCacheService _redisCacheService;

    /// <summary>
    /// 创建 Token 会话服务。
    /// </summary>
    public RedisTokenSessionService(IRedisCacheService redisCacheService)
    {
        _redisCacheService = redisCacheService;
    }

    /// <summary>
    /// 保存 Admin JWT 会话。
    /// </summary>
    public async Task SaveAdminSessionAsync(TokenSession session, CancellationToken cancellationToken = default)
    {
        await _redisCacheService.SetAsync(AdminTokenSessionRedisKeys.GetSessionKey(session.SessionId), session, GetExpire(session.ExpiresAt), cancellationToken);
    }

    /// <summary>
    /// 缓存 Admin 刷新 Token 和当前会话的关系。
    /// </summary>
    public async Task SaveAdminRefreshTokenAsync(string refreshToken, RefreshTokenSession session, CancellationToken cancellationToken = default)
    {
        await _redisCacheService.SetAsync(AdminTokenSessionRedisKeys.GetRefreshKey(refreshToken), session, GetExpire(session.RefreshTokenExpiresAt), cancellationToken);
    }

    /// <summary>
    /// 获取 Admin 刷新 Token 对应的会话缓存。
    /// </summary>
    public async Task<RefreshTokenSession?> GetAdminRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        return await _redisCacheService.GetAsync<RefreshTokenSession>(AdminTokenSessionRedisKeys.GetRefreshKey(refreshToken), cancellationToken);
    }

    /// <summary>
    /// 删除 Admin 刷新 Token 缓存。
    /// </summary>
    public async Task RemoveAdminRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        await _redisCacheService.RemoveAsync(AdminTokenSessionRedisKeys.GetRefreshKey(refreshToken), cancellationToken);
    }

    /// <summary>
    /// 吊销 Admin JWT 会话，并把当前 JWT 编号加入黑名单。
    /// </summary>
    public async Task RevokeAdminSessionAsync(Guid sessionId, string jwtId, DateTime expiresAt, CancellationToken cancellationToken = default)
    {
        await _redisCacheService.RemoveAsync(AdminTokenSessionRedisKeys.GetSessionKey(sessionId), cancellationToken);
        await _redisCacheService.SetStringAsync(AdminTokenSessionRedisKeys.GetBlacklistKey(jwtId), "1", GetExpire(expiresAt), cancellationToken);
    }

    /// <summary>
    /// 计算缓存过期时间。
    /// </summary>
    private static TimeSpan GetExpire(DateTime expiresAt)
    {
        var expire = expiresAt - DateTime.UtcNow;
        return expire <= TimeSpan.Zero ? TimeSpan.FromSeconds(1) : expire;
    }
}
