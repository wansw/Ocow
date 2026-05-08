using Ocow.Identity.Application.Models;
using Ocow.InternalAuth.Models;

namespace Ocow.Identity.Application.Interfaces;

/// <summary>
/// Token 会话服务接口，用于管理 Admin JWT Redis 会话、刷新 Token 缓存和 JWT 黑名单。
/// </summary>
public interface IRedisTokenSessionService
{
    /// <summary>
    /// 保存 Admin JWT 会话。
    /// </summary>
    Task SaveAdminSessionAsync(TokenSession session, CancellationToken cancellationToken = default);

    /// <summary>
    /// 缓存 Admin 刷新 Token 和当前会话的关系。
    /// </summary>
    Task SaveAdminRefreshTokenAsync(string refreshToken, RefreshTokenSession session, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取 Admin 刷新 Token 对应的会话缓存。
    /// </summary>
    Task<RefreshTokenSession?> GetAdminRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除 Admin 刷新 Token 缓存。
    /// </summary>
    Task RemoveAdminRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// 吊销 Admin JWT 会话，并把当前 JWT 编号加入黑名单。
    /// </summary>
    Task RevokeAdminSessionAsync(Guid sessionId, string jwtId, DateTime expiresAt, CancellationToken cancellationToken = default);
}
