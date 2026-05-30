using System.Security.Claims;
using Ocow.Cache.Interfaces;
using Microsoft.Extensions.Options;
using Ocow.EntityFrameworkCore.Abstractions;
using Ocow.Identity.Application.Dtos;
using Ocow.Identity.Application.Interfaces;
using Ocow.Identity.Application.Options;
using Ocow.Identity.Application.Services;
using Ocow.Identity.Domain.Models;
using Ocow.Shared.Dtos;
using Ocow.WeChat.Abstractions.Dtos;
using Ocow.WeChat.Abstractions.Interfaces;

namespace Ocow.Tests.Unit;

/// <summary>
/// 客户端微信授权登录单元测试，用于验证小程序和公众号登录会使用微信接口返回的身份。
/// </summary>
public class ClientWechatAuthAppServiceTests
{
    /// <summary>
    /// 验证小程序登录会使用 code2Session 返回的 openid 创建会员身份。
    /// </summary>
    [Fact]
    public async Task WechatLoginAsync_WhenMiniProgramCodeValid_ShouldCreateIdentityFromWechatOpenId()
    {
        var repository = new FakeIdentityRepository();
        var miniProgramClient = new FakeWechatMiniProgramClient(new WechatCode2SessionResDto
        {
            OpenId = "wx-mini-openid",
            UnionId = "wx-unionid"
        });
        var service = CreateService(repository, miniProgramClient, new FakeWechatOfficialAccountClient());

        var token = await service.WechatLoginAsync(new WechatLoginReqDto
        {
            Code = "login-code"
        });

        Assert.False(string.IsNullOrWhiteSpace(token.AccessToken));
        Assert.Single(repository.MemberIdentities);
        Assert.Equal("wx-mini-openid", repository.MemberIdentities[0].OpenId);
        Assert.Equal("wx-unionid", repository.MemberIdentities[0].UnionId);
        Assert.Equal("login-code", miniProgramClient.LastCode);
    }

    /// <summary>
    /// 验证公众号登录会使用网页授权返回的 openid 创建会员身份。
    /// </summary>
    [Fact]
    public async Task WechatOfficialAccountLoginAsync_WhenCodeValid_ShouldCreateIdentityFromWechatOpenId()
    {
        var repository = new FakeIdentityRepository();
        var officialAccountClient = new FakeWechatOfficialAccountClient(new WechatOfficialAccountOAuthResDto
        {
            OpenId = "wx-official-openid",
            UnionId = "wx-unionid"
        });
        var service = CreateService(repository, new FakeWechatMiniProgramClient(), officialAccountClient);
        var state = await service.CreateWechatOfficialAccountLoginStateAsync();

        var token = await service.WechatOfficialAccountLoginAsync(new WechatOfficialAccountLoginReqDto
        {
            Code = "oauth-code",
            State = state.State
        });

        Assert.False(string.IsNullOrWhiteSpace(token.AccessToken));
        Assert.Single(repository.MemberIdentities);
        Assert.Equal("wx-official-openid", repository.MemberIdentities[0].OpenId);
        Assert.Equal("wx-unionid", repository.MemberIdentities[0].UnionId);
        Assert.Equal("oauth-code", officialAccountClient.LastCode);
    }

    /// <summary>
    /// 验证小程序先登录后，公众号相同 unionid 登录会复用同一会员。
    /// </summary>
    [Fact]
    public async Task WechatOfficialAccountLoginAsync_WhenMiniProgramLoggedInFirstWithSameUnionId_ShouldNotCreateSecondMember()
    {
        var repository = new FakeIdentityRepository();
        var miniProgramClient = new FakeWechatMiniProgramClient(new WechatCode2SessionResDto
        {
            OpenId = "wx-mini-openid",
            UnionId = "wx-unionid"
        });
        var officialAccountClient = new FakeWechatOfficialAccountClient(new WechatOfficialAccountOAuthResDto
        {
            OpenId = "wx-official-openid",
            UnionId = "wx-unionid"
        });
        var service = CreateService(repository, miniProgramClient, officialAccountClient);

        await service.WechatLoginAsync(new WechatLoginReqDto
        {
            Code = "mini-code"
        });
        var state = await service.CreateWechatOfficialAccountLoginStateAsync();
        await service.WechatOfficialAccountLoginAsync(new WechatOfficialAccountLoginReqDto
        {
            Code = "official-code",
            State = state.State
        });

        Assert.Equal(2, repository.MemberIdentities.Count);
        Assert.Single(repository.MemberIdentities.Select(x => x.MemberId).Distinct());
        Assert.Contains(repository.MemberIdentities, x => x.OpenId == "wx-mini-openid");
        Assert.Contains(repository.MemberIdentities, x => x.OpenId == "wx-official-openid");
    }

    /// <summary>
    /// 验证公众号网页登录传入非法 state 时会拒绝登录。
    /// </summary>
    [Fact]
    public async Task WechatOfficialAccountLoginAsync_WhenStateInvalid_ShouldRejectLogin()
    {
        var repository = new FakeIdentityRepository();
        var officialAccountClient = new FakeWechatOfficialAccountClient(new WechatOfficialAccountOAuthResDto
        {
            OpenId = "wx-official-openid",
            UnionId = "wx-unionid"
        });
        var service = CreateService(repository, new FakeWechatMiniProgramClient(), officialAccountClient);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.WechatOfficialAccountLoginAsync(new WechatOfficialAccountLoginReqDto
        {
            Code = "oauth-code",
            State = "invalid-state"
        }));
        Assert.Empty(repository.MemberIdentities);
        Assert.Null(officialAccountClient.LastCode);
    }

    /// <summary>
    /// 验证公众号网页登录 state 被使用一次后无法重放。
    /// </summary>
    [Fact]
    public async Task WechatOfficialAccountLoginAsync_WhenStateReused_ShouldRejectReplay()
    {
        var repository = new FakeIdentityRepository();
        var officialAccountClient = new FakeWechatOfficialAccountClient(new WechatOfficialAccountOAuthResDto
        {
            OpenId = "wx-official-openid",
            UnionId = "wx-unionid"
        });
        var service = CreateService(repository, new FakeWechatMiniProgramClient(), officialAccountClient);
        var state = await service.CreateWechatOfficialAccountLoginStateAsync();

        await service.WechatOfficialAccountLoginAsync(new WechatOfficialAccountLoginReqDto
        {
            Code = "oauth-code",
            State = state.State
        });

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.WechatOfficialAccountLoginAsync(new WechatOfficialAccountLoginReqDto
        {
            Code = "replay-code",
            State = state.State
        }));
        Assert.Single(repository.MemberIdentities);
        Assert.Equal(1, officialAccountClient.CallCount);
    }

    /// <summary>
    /// 创建客户端认证应用服务测试实例。
    /// </summary>
    private static ClientAuthAppService CreateService(
        FakeIdentityRepository repository,
        IWechatMiniProgramClient miniProgramClient,
        IWechatOfficialAccountClient officialAccountClient)
    {
        return new ClientAuthAppService(
            repository,
            new TokenService(Options.Create(new JwtTokenOption
            {
                Secret = "UnitTestIdentityJwtSecret@2026-EnoughLong"
            })),
            new FakeUnitOfWork(),
            new FakeCacheService(),
            miniProgramClient,
            officialAccountClient);
    }

    private class FakeWechatMiniProgramClient : IWechatMiniProgramClient
    {
        private readonly WechatCode2SessionResDto _response;

        public FakeWechatMiniProgramClient()
            : this(new WechatCode2SessionResDto())
        {
        }

        public FakeWechatMiniProgramClient(WechatCode2SessionResDto response)
        {
            _response = response;
        }

        public string? LastCode { get; private set; }

        /// <summary>
        /// 返回预设的小程序登录结果。
        /// </summary>
        public Task<WechatCode2SessionResDto> Code2SessionAsync(WechatCode2SessionReqDto reqDto, CancellationToken cancellationToken = default)
        {
            LastCode = reqDto.Code;
            return Task.FromResult(_response);
        }
    }

    private class FakeWechatOfficialAccountClient : IWechatOfficialAccountClient
    {
        private readonly WechatOfficialAccountOAuthResDto _response;

        public FakeWechatOfficialAccountClient()
            : this(new WechatOfficialAccountOAuthResDto())
        {
        }

        public FakeWechatOfficialAccountClient(WechatOfficialAccountOAuthResDto response)
        {
            _response = response;
        }

        public string? LastCode { get; private set; }

        public int CallCount { get; private set; }

        /// <summary>
        /// 返回预设的公众号网页授权结果。
        /// </summary>
        public Task<WechatOfficialAccountOAuthResDto> GetOAuthAccessTokenAsync(WechatOfficialAccountOAuthReqDto reqDto, CancellationToken cancellationToken = default)
        {
            LastCode = reqDto.Code;
            CallCount++;
            return Task.FromResult(_response);
        }
    }

    private class FakeIdentityRepository : IIdentityRepository
    {
        public List<MemberIdentity> MemberIdentities { get; } = new();

        public List<RefreshToken> RefreshTokens { get; } = new();

        /// <summary>
        /// 测试中不查询管理员账号。
        /// </summary>
        public Task<AdminUser?> GetAdminUserByNameAsync(string userName, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<AdminUser?>(null);
        }

        /// <summary>
        /// 测试中不查询管理员权限。
        /// </summary>
        public Task<IReadOnlyList<string>> GetAdminPermissionCodesAsync(Guid adminUserId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<string>>(Array.Empty<string>());
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

        /// <summary>
        /// 根据 openid 查询测试内存中的会员微信身份。
        /// </summary>
        public Task<MemberIdentity?> GetMemberIdentityByOpenIdAsync(string openId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(MemberIdentities.FirstOrDefault(x => x.OpenId == openId));
        }

        /// <summary>
        /// 根据 unionid 查询测试内存中的会员微信身份。
        /// </summary>
        public Task<MemberIdentity?> GetMemberIdentityByUnionIdAsync(string unionId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(MemberIdentities.FirstOrDefault(x => x.UnionId == unionId));
        }

        /// <summary>
        /// 保存测试内存中的会员微信身份。
        /// </summary>
        public Task SaveMemberIdentityAsync(MemberIdentity memberIdentity, CancellationToken cancellationToken = default)
        {
            MemberIdentities.Add(memberIdentity);
            return Task.CompletedTask;
        }

        /// <summary>
        /// 保存测试内存中的刷新 Token。
        /// </summary>
        public Task SaveRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
        {
            RefreshTokens.Add(refreshToken);
            return Task.CompletedTask;
        }

        /// <summary>
        /// 查询测试内存中的有效刷新 Token。
        /// </summary>
        public Task<RefreshToken?> GetRefreshTokenAsync(string token, string scope, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(RefreshTokens.FirstOrDefault(x =>
                x.Token == token &&
                x.Scope == scope &&
                x.RevokedAt == null &&
                x.ExpiresAt > DateTime.UtcNow));
        }

        /// <summary>
        /// 吊销测试内存中的刷新 Token。
        /// </summary>
        public Task RevokeRefreshTokenAsync(string token, string scope, CancellationToken cancellationToken = default)
        {
            var refreshToken = RefreshTokens.FirstOrDefault(x => x.Token == token && x.Scope == scope);
            if (refreshToken is not null)
            {
                refreshToken.RevokedAt = DateTime.UtcNow;
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// 测试中忽略登录日志写入。
        /// </summary>
        public Task AddLoginLogAsync(LoginLog loginLog, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    private class FakeCacheService : ICacheService
    {
        private readonly Dictionary<string, string> _items = new();

        /// <summary>
        /// 写入测试字符串缓存。
        /// </summary>
        public Task SetStringAsync(string key, string value, TimeSpan? expire = null, CancellationToken cancellationToken = default)
        {
            _items[key] = value;
            return Task.CompletedTask;
        }

        /// <summary>
        /// 读取测试字符串缓存。
        /// </summary>
        public Task<string?> GetStringAsync(string key, CancellationToken cancellationToken = default)
        {
            _items.TryGetValue(key, out var value);
            return Task.FromResult<string?>(value);
        }

        /// <summary>
        /// 写入测试对象缓存。
        /// </summary>
        public Task SetAsync<T>(string key, T value, TimeSpan? expire = null, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// 读取测试对象缓存。
        /// </summary>
        public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// 判断测试缓存是否存在。
        /// </summary>
        public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_items.ContainsKey(key));
        }

        /// <summary>
        /// 设置测试缓存过期时间，内存测试中不处理实际过期。
        /// </summary>
        public Task<bool> ExpireAsync(string key, TimeSpan expire, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_items.ContainsKey(key));
        }

        /// <summary>
        /// 删除测试缓存并返回是否删除成功。
        /// </summary>
        public Task<bool> RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_items.Remove(key));
        }

        /// <summary>
        /// 按前缀删除测试缓存。
        /// </summary>
        public Task<long> RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
        {
            var keys = _items.Keys.Where(x => x.StartsWith(prefix, StringComparison.Ordinal)).ToArray();
            foreach (var key in keys)
            {
                _items.Remove(key);
            }

            return Task.FromResult((long)keys.Length);
        }
    }

    private class FakeUnitOfWork : IUnitOfWork
    {
        /// <summary>
        /// 测试中没有需要提交的数据库上下文。
        /// </summary>
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(0);
        }

        /// <summary>
        /// 测试中直接执行无返回值事务委托。
        /// </summary>
        public Task ExecuteInTransactionAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken = default)
        {
            return action(cancellationToken);
        }

        /// <summary>
        /// 测试中直接执行带返回值事务委托。
        /// </summary>
        public Task<TResult> ExecuteInTransactionAsync<TResult>(Func<CancellationToken, Task<TResult>> action, CancellationToken cancellationToken = default)
        {
            return action(cancellationToken);
        }
    }
}
