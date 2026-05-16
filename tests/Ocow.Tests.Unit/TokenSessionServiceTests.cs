using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Ocow.Cache.Interfaces;
using Ocow.Identity.Application.Services;
using Ocow.Auth.Interfaces;
using Ocow.Auth.Models;
using Ocow.Auth.Services;

namespace Ocow.Tests.Unit;

/// <summary>
/// Token 会话服务测试，用于验证 Identity 写入会话后 InternalAuth 可以校验 Admin JWT。
/// </summary>
public class TokenSessionServiceTests
{
    /// <summary>
    /// 验证 Admin JWT 对应 Redis 会话存在时校验通过。
    /// </summary>
    [Fact]
    public async Task ValidateAdminToken_WhenSessionExists_ShouldReturnTrue()
    {
        var cache = new FakeCacheService();
        var sessionService = new RedisTokenSessionService(cache);
        IAdminTokenSessionValidator validator = new RedisAdminTokenSessionValidator(cache);
        var subjectId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        const string jwtId = "jwt-1";

        await sessionService.SaveAdminSessionAsync(new TokenSession
        {
            SubjectId = subjectId,
            SessionId = sessionId,
            Scope = "admin",
            JwtId = jwtId,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10)
        });

        var result = await validator.ValidateAsync(CreatePrincipal(subjectId, sessionId, jwtId));

        Assert.True(result);
    }

    /// <summary>
    /// 验证 Admin JWT 已加入黑名单时校验失败。
    /// </summary>
    [Fact]
    public async Task ValidateAdminToken_WhenJwtBlacklisted_ShouldReturnFalse()
    {
        var cache = new FakeCacheService();
        var sessionService = new RedisTokenSessionService(cache);
        IAdminTokenSessionValidator validator = new RedisAdminTokenSessionValidator(cache);
        var subjectId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        const string jwtId = "jwt-2";

        await sessionService.SaveAdminSessionAsync(new TokenSession
        {
            SubjectId = subjectId,
            SessionId = sessionId,
            Scope = "admin",
            JwtId = jwtId,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10)
        });
        await sessionService.RevokeAdminSessionAsync(sessionId, jwtId, DateTime.UtcNow.AddMinutes(10));

        var result = await validator.ValidateAsync(CreatePrincipal(subjectId, sessionId, jwtId));

        Assert.False(result);
    }

    /// <summary>
    /// 创建包含 Admin 会话标识的测试用户主体。
    /// </summary>
    private static ClaimsPrincipal CreatePrincipal(Guid subjectId, Guid sessionId, string jwtId)
    {
        return new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, subjectId.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, jwtId),
            new Claim("sid", sessionId.ToString()),
            new Claim("scope", "admin")
        }, "test"));
    }

    private class FakeCacheService : ICacheService
    {
        private readonly Dictionary<string, string> _strings = new();
        private readonly Dictionary<string, object> _objects = new();

        public Task SetStringAsync(string key, string value, TimeSpan? expire = null, CancellationToken cancellationToken = default)
        {
            _strings[key] = value;
            return Task.CompletedTask;
        }

        public Task<string?> GetStringAsync(string key, CancellationToken cancellationToken = default)
        {
            _strings.TryGetValue(key, out var value);
            return Task.FromResult(value);
        }

        public Task SetAsync<T>(string key, T value, TimeSpan? expire = null, CancellationToken cancellationToken = default)
        {
            _objects[key] = value!;
            return Task.CompletedTask;
        }

        public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_objects.TryGetValue(key, out var value) ? (T)value : default);
        }

        public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_strings.ContainsKey(key) || _objects.ContainsKey(key));
        }

        public Task<bool> ExpireAsync(string key, TimeSpan expire, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_strings.ContainsKey(key) || _objects.ContainsKey(key));
        }

        public Task<bool> RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_strings.Remove(key) || _objects.Remove(key));
        }

        public Task<long> RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
        {
            var stringKeys = _strings.Keys.Where(x => x.StartsWith(prefix, StringComparison.Ordinal)).ToList();
            var objectKeys = _objects.Keys.Where(x => x.StartsWith(prefix, StringComparison.Ordinal)).ToList();
            stringKeys.ForEach(key => _strings.Remove(key));
            objectKeys.ForEach(key => _objects.Remove(key));
            return Task.FromResult((long)stringKeys.Count + objectKeys.Count);
        }
    }
}
