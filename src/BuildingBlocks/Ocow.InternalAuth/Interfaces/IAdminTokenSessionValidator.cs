using System.Security.Claims;

namespace Ocow.InternalAuth.Interfaces;

/// <summary>
/// Admin Token 会话校验接口，用于认证事件中校验 Redis 集中式会话是否有效。
/// </summary>
public interface IAdminTokenSessionValidator
{
    /// <summary>
    /// 校验 Admin JWT 是否仍存在有效 Redis 会话。
    /// </summary>
    Task<bool> ValidateAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default);
}
