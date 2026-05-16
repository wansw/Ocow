using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Ocow.Auth.Constants;
using Ocow.Auth.Interfaces;
using Ocow.Auth.Models;
using Ocow.Redis.Interfaces;

namespace Ocow.Auth.Services;

/// <summary>
/// Redis Admin Token 会话校验器，用于各微服务在认证事件中校验后台登录态。
/// </summary>
public class RedisAdminTokenSessionValidator : IAdminTokenSessionValidator
{
    private const string AdminScope = "admin";

    private readonly IRedisCacheService _redisCacheService;

    /// <summary>
    /// 创建 Redis Admin Token 会话校验器。
    /// </summary>
    public RedisAdminTokenSessionValidator(IRedisCacheService redisCacheService)
    {
        _redisCacheService = redisCacheService;
    }

    /// <summary>
    /// 校验 Admin JWT 是否仍存在有效 Redis 会话。
    /// </summary>
    public async Task<bool> ValidateAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default)
    {
        var scope = principal.FindFirstValue("scope");
        if (!string.Equals(scope, AdminScope, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var subjectIdValue = principal.FindFirstValue(JwtRegisteredClaimNames.Sub) ??
                             principal.FindFirstValue(ClaimTypes.NameIdentifier);
        var sessionIdValue = principal.FindFirstValue("sid");
        var jwtId = principal.FindFirstValue(JwtRegisteredClaimNames.Jti);
        if (!Guid.TryParse(subjectIdValue, out var subjectId) ||
            !Guid.TryParse(sessionIdValue, out var sessionId) ||
            string.IsNullOrWhiteSpace(jwtId))
        {
            return false;
        }

        if (await _redisCacheService.ExistsAsync(AdminTokenSessionRedisKeys.GetBlacklistKey(jwtId), cancellationToken))
        {
            return false;
        }

        var session = await _redisCacheService.GetAsync<TokenSession>(AdminTokenSessionRedisKeys.GetSessionKey(sessionId), cancellationToken);
        return session is not null &&
               session.SubjectId == subjectId &&
               session.SessionId == sessionId &&
               string.Equals(session.Scope, AdminScope, StringComparison.OrdinalIgnoreCase) &&
               string.Equals(session.JwtId, jwtId, StringComparison.Ordinal);
    }
}
