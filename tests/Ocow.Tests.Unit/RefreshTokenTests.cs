using Microsoft.Extensions.Options;
using Ocow.Identity.Application.Dtos;
using Ocow.Identity.Application.Interfaces;
using Ocow.Identity.Application.Options;
using Ocow.Identity.Application.Services;
using Ocow.Identity.Domain.Models;
using Ocow.Shared.Dtos;

namespace Ocow.Tests.Unit;

/// <summary>
/// 刷新 Token 单元测试，用于验证刷新时会轮换并吊销旧 Token。
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
        var oldToken = new RefreshTokenModel
        {
            Id = Guid.NewGuid(),
            SubjectId = Guid.NewGuid(),
            Scope = "admin",
            Token = "old-refresh-token",
            ExpiresAt = DateTime.UtcNow.AddDays(1)
        };
        repository.RefreshTokens.Add(oldToken);
        repository.PermissionCodes.Add("order.ship");

        var service = new AdminAuthAppService(repository, new TokenService(Options.Create(new JwtTokenOption
        {
            Secret = "UnitTestIdentityJwtSecret@2026-EnoughLong"
        })));

        var result = await service.RefreshTokenAsync(new RefreshTokenReqDto { RefreshToken = oldToken.Token });

        Assert.NotEqual(oldToken.Token, result.RefreshToken);
        Assert.NotNull(oldToken.RevokedAt);
        Assert.Equal(2, repository.RefreshTokens.Count);
    }

    private class FakeIdentityRepository : IIdentityRepository
    {
        public List<RefreshTokenModel> RefreshTokens { get; } = new();

        public List<string> PermissionCodes { get; } = new();

        public Task<AdminUserModel?> GetAdminUserByNameAsync(string userName, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<AdminUserModel?>(null);
        }

        public Task<IReadOnlyList<string>> GetAdminPermissionCodesAsync(Guid adminUserId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<string>>(PermissionCodes);
        }

        public Task<PageResDto<AdminUserModel>> GetAdminUsersAsync(PageReqDto reqDto, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task AddAdminUserAsync(AdminUserModel adminUser, IReadOnlyCollection<Guid> roleIds, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task DisableAdminUserAsync(Guid id, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<IReadOnlyList<RoleModel>> GetRolesAsync(CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<RoleModel> SaveRoleAsync(RoleModel role, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<IReadOnlyList<PermissionModel>> GetPermissionsAsync(CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task BindRolePermissionsAsync(Guid roleId, IReadOnlyCollection<Guid> permissionIds, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<MemberIdentityModel?> GetMemberIdentityByOpenIdAsync(string openId, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task SaveMemberIdentityAsync(MemberIdentityModel memberIdentity, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task SaveRefreshTokenAsync(RefreshTokenModel refreshToken, CancellationToken cancellationToken = default)
        {
            RefreshTokens.Add(refreshToken);
            return Task.CompletedTask;
        }

        public Task<RefreshTokenModel?> GetRefreshTokenAsync(string token, string scope, CancellationToken cancellationToken = default)
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

        public Task AddLoginLogAsync(LoginLogModel loginLog, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
