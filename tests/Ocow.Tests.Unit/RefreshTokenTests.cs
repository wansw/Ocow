using Microsoft.Extensions.Options;
using Ocow.EntityFrameworkCore.Abstractions;
using Ocow.Identity.Application.Dtos;
using Ocow.Identity.Application.Interfaces;
using Ocow.Identity.Application.Models;
using Ocow.Identity.Application.Options;
using Ocow.Identity.Application.Services;
using Ocow.Identity.Domain.Models;
using Ocow.InternalAuth.Models;
using Ocow.Shared.Dtos;

namespace Ocow.Tests.Unit;

/// <summary>
/// 刷新 Token 单元测试，用于验证刷新时会轮换并吊销 Token。
/// </summary>
public class RefreshTokenTests
{
    /// <summary>
    /// 验证管理员刷新 Token 会吊销旧 Token 并保存新 Token。
    /// </summary>
    [Fact]
    public async Task RefreshToken_WhenAdminTokenValid_ShouldRotateRefreshToken()
    {
        var repository = new FakeIdentityRepository();
        var oldToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            SubjectId = Guid.NewGuid(),
            Scope = "admin",
            Token = "old-refresh-token",
            ExpiresAt = DateTime.UtcNow.AddDays(1)
        };
        repository.RefreshTokens.Add(oldToken);
        repository.PermissionCodes.Add("order.ship");

        var tokenSessionService = new FakeTokenSessionService();
        await tokenSessionService.SaveAdminRefreshTokenAsync(oldToken.Token, new RefreshTokenSession
        {
            SubjectId = oldToken.SubjectId,
            SessionId = Guid.NewGuid(),
            JwtId = "old-jti",
            AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(30),
            RefreshTokenExpiresAt = oldToken.ExpiresAt
        });

        var service = new AdminAuthAppService(repository, new TokenService(Options.Create(new JwtTokenOption
        {
            Secret = "UnitTestIdentityJwtSecret@2026-EnoughLong"
        })), tokenSessionService, new FakeUnitOfWork());

        var result = await service.RefreshTokenAsync(new RefreshTokenReqDto { RefreshToken = oldToken.Token });

        Assert.NotEqual(oldToken.Token, result.RefreshToken);
        Assert.NotNull(oldToken.RevokedAt);
        Assert.Equal(2, repository.RefreshTokens.Count);
        Assert.True(tokenSessionService.RevokedSessionCount > 0);
    }

    private class FakeTokenSessionService : IRedisTokenSessionService
    {
        private readonly Dictionary<string, RefreshTokenSession> _refreshTokens = new();

        public int RevokedSessionCount { get; private set; }

        public Task SaveAdminSessionAsync(TokenSession session, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task SaveAdminRefreshTokenAsync(string refreshToken, RefreshTokenSession session, CancellationToken cancellationToken = default)
        {
            _refreshTokens[refreshToken] = session;
            return Task.CompletedTask;
        }

        public Task<RefreshTokenSession?> GetAdminRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            _refreshTokens.TryGetValue(refreshToken, out var session);
            return Task.FromResult(session);
        }

        public Task RemoveAdminRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            _refreshTokens.Remove(refreshToken);
            return Task.CompletedTask;
        }

        public Task RevokeAdminSessionAsync(Guid sessionId, string jwtId, DateTime expiresAt, CancellationToken cancellationToken = default)
        {
            RevokedSessionCount++;
            return Task.CompletedTask;
        }

    }

    private class FakeIdentityRepository : IIdentityRepository
    {
        public List<RefreshToken> RefreshTokens { get; } = new();

        public List<string> PermissionCodes { get; } = new();

        public Task<AdminUser?> GetAdminUserByNameAsync(string userName, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<AdminUser?>(null);
        }

        public Task<IReadOnlyList<string>> GetAdminPermissionCodesAsync(Guid adminUserId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<string>>(PermissionCodes);
        }

        public Task<PageResDto<AdminUser>> GetAdminUsersAsync(PageReqDto reqDto, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task AddAdminUserAsync(AdminUser adminUser, IReadOnlyCollection<Guid> roleIds, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task DisableAdminUserAsync(Guid id, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<IReadOnlyList<Role>> GetRolesAsync(CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<Role> SaveRoleAsync(Role role, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<IReadOnlyList<Permission>> GetPermissionsAsync(CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<IReadOnlyList<Menu>> GetMenusAsync(CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<IReadOnlyList<Menu>> GetAdminMenusAsync(Guid adminUserId, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<Menu> SaveMenuAsync(Menu menu, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task BindRolePermissionsAsync(Guid roleId, IReadOnlyCollection<Guid> permissionIds, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<MemberIdentity?> GetMemberIdentityByOpenIdAsync(string openId, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task SaveMemberIdentityAsync(MemberIdentity memberIdentity, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task SaveRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
        {
            RefreshTokens.Add(refreshToken);
            return Task.CompletedTask;
        }

        public Task<RefreshToken?> GetRefreshTokenAsync(string token, string scope, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(RefreshTokens.FirstOrDefault(x =>
                x.Token == token &&
                x.Scope == scope &&
                x.RevokedAt == null &&
                x.ExpiresAt > DateTime.UtcNow));
        }

        public Task RevokeRefreshTokenAsync(string token, string scope, CancellationToken cancellationToken = default)
        {
            var refreshToken = RefreshTokens.FirstOrDefault(x => x.Token == token && x.Scope == scope);
            if (refreshToken is not null)
            {
                refreshToken.RevokedAt = DateTime.UtcNow;
            }

            return Task.CompletedTask;
        }

        public Task AddLoginLogAsync(LoginLog loginLog, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    private class FakeUnitOfWork : IUnitOfWork
    {
        /// <summary>
        /// 保存测试上下文变更，内存仓储无需实际提交。
        /// </summary>
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(0);
        }

        /// <summary>
        /// 在测试中直接执行事务委托。
        /// </summary>
        public Task ExecuteInTransactionAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken = default)
        {
            return action(cancellationToken);
        }

        /// <summary>
        /// 在测试中直接执行带返回值的事务委托。
        /// </summary>
        public Task<TResult> ExecuteInTransactionAsync<TResult>(Func<CancellationToken, Task<TResult>> action, CancellationToken cancellationToken = default)
        {
            return action(cancellationToken);
        }
    }
}
