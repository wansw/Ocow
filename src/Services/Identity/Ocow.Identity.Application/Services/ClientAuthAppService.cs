using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Ocow.Cache.Interfaces;
using Ocow.EntityFrameworkCore.Abstractions;
using Ocow.Identity.Application.Dtos;
using Ocow.Identity.Application.Interfaces;
using Ocow.Identity.Domain.Models;
using Ocow.WeChat.Abstractions.Dtos;
using Ocow.WeChat.Abstractions.Interfaces;

namespace Ocow.Identity.Application.Services;

/// <summary>
/// 小程序认证应用服务，用于微信登录和 Customer JWT 签发。
/// </summary>
public class ClientAuthAppService : IClientAuthAppService
{
    private const string WechatOfficialAccountStateCacheKeyPrefix = "identity:wechat:official-account:state";
    private static readonly TimeSpan WechatOfficialAccountStateExpire = TimeSpan.FromMinutes(5);

    private readonly ICacheService _cacheService;
    private readonly IIdentityRepository _repository;
    private readonly ITokenService _tokenService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IWechatMiniProgramClient _wechatMiniProgramClient;
    private readonly IWechatOfficialAccountClient _wechatOfficialAccountClient;

    /// <summary>
    /// 创建小程序认证应用服务。
    /// </summary>
    public ClientAuthAppService(
        IIdentityRepository repository,
        ITokenService tokenService,
        IUnitOfWork unitOfWork,
        ICacheService cacheService,
        IWechatMiniProgramClient wechatMiniProgramClient,
        IWechatOfficialAccountClient wechatOfficialAccountClient)
    {
        _repository = repository;
        _tokenService = tokenService;
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
        _wechatMiniProgramClient = wechatMiniProgramClient;
        _wechatOfficialAccountClient = wechatOfficialAccountClient;
    }

    /// <summary>
    /// 小程序微信登录并签发 Customer JWT。
    /// </summary>
    public async Task<AuthTokenResDto> WechatLoginAsync(WechatLoginReqDto reqDto, CancellationToken cancellationToken = default)
    {
        var session = await _wechatMiniProgramClient.Code2SessionAsync(new WechatCode2SessionReqDto
        {
            Code = reqDto.Code
        }, cancellationToken);

        return await SignInWechatMemberAsync(session.OpenId, session.UnionId, cancellationToken);
    }

    /// <summary>
    /// 创建公众号网页登录一次性 state。
    /// </summary>
    public async Task<WechatOfficialAccountStateResDto> CreateWechatOfficialAccountLoginStateAsync(CancellationToken cancellationToken = default)
    {
        var state = CreateWechatOfficialAccountState();
        await _cacheService.SetStringAsync(BuildWechatOfficialAccountStateCacheKey(state), "1", WechatOfficialAccountStateExpire, cancellationToken);

        return new WechatOfficialAccountStateResDto
        {
            State = state,
            ExpiresAt = DateTime.UtcNow.Add(WechatOfficialAccountStateExpire)
        };
    }

    /// <summary>
    /// 公众号微信登录并签发 Customer JWT。
    /// </summary>
    public async Task<AuthTokenResDto> WechatOfficialAccountLoginAsync(WechatOfficialAccountLoginReqDto reqDto, CancellationToken cancellationToken = default)
    {
        await ValidateWechatOfficialAccountStateAsync(reqDto.State, cancellationToken);

        var oauth = await _wechatOfficialAccountClient.GetOAuthAccessTokenAsync(new WechatOfficialAccountOAuthReqDto
        {
            Code = reqDto.Code
        }, cancellationToken);

        return await SignInWechatMemberAsync(oauth.OpenId, oauth.UnionId, cancellationToken);
    }

    /// <summary>
    /// 根据微信 unionid 或 openid 登录或创建会员身份并签发 Customer JWT。
    /// </summary>
    private async Task<AuthTokenResDto> SignInWechatMemberAsync(string openId, string? unionId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(openId))
        {
            throw new InvalidOperationException("微信 openid 不能为空。");
        }

        return await _unitOfWork.ExecuteInTransactionAsync(async tokenCancellationToken =>
        {
            var identity = await GetOrCreateWechatMemberIdentityAsync(openId, unionId, tokenCancellationToken);

            var claims = new List<Claim>
            {
                new Claim("memberId", identity.MemberId.ToString()),
                new Claim("openid", identity.OpenId)
            };
            if (!string.IsNullOrWhiteSpace(identity.UnionId))
            {
                claims.Add(new Claim("unionid", identity.UnionId));
            }

            var token = _tokenService.IssueToken(identity.MemberId, "client", Array.Empty<string>(), claims);
            await _repository.SaveRefreshTokenAsync(CreateRefreshToken(identity.MemberId, "client", token.RefreshToken), tokenCancellationToken);

            return token;
        }, cancellationToken);
    }

    /// <summary>
    /// 根据 unionid 优先、openid 兜底获取或创建当前微信身份。
    /// </summary>
    private async Task<MemberIdentity> GetOrCreateWechatMemberIdentityAsync(string openId, string? unionId, CancellationToken cancellationToken)
    {
        var identityByOpenId = await _repository.GetMemberIdentityByOpenIdAsync(openId, cancellationToken);
        if (string.IsNullOrWhiteSpace(unionId))
        {
            return identityByOpenId ?? await CreateWechatMemberIdentityAsync(openId, null, Guid.NewGuid(), cancellationToken);
        }

        var identityByUnionId = await _repository.GetMemberIdentityByUnionIdAsync(unionId, cancellationToken);
        if (identityByUnionId is null)
        {
            if (identityByOpenId is not null)
            {
                identityByOpenId.UnionId = unionId;
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return identityByOpenId;
            }

            return await CreateWechatMemberIdentityAsync(openId, unionId, Guid.NewGuid(), cancellationToken);
        }

        if (identityByOpenId is null)
        {
            return await CreateWechatMemberIdentityAsync(openId, unionId, identityByUnionId.MemberId, cancellationToken);
        }

        if (identityByOpenId.MemberId != identityByUnionId.MemberId)
        {
            identityByOpenId.MemberId = identityByUnionId.MemberId;
            identityByOpenId.UnionId = unionId;
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        else if (string.IsNullOrWhiteSpace(identityByOpenId.UnionId))
        {
            identityByOpenId.UnionId = unionId;
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return identityByOpenId;
    }

    /// <summary>
    /// 创建微信会员身份并保存。
    /// </summary>
    private async Task<MemberIdentity> CreateWechatMemberIdentityAsync(string openId, string? unionId, Guid memberId, CancellationToken cancellationToken)
    {
        var identity = new MemberIdentity
        {
            Id = Guid.NewGuid(),
            MemberId = memberId,
            OpenId = openId,
            UnionId = unionId
        };
        await _repository.SaveMemberIdentityAsync(identity, cancellationToken);
        return identity;
    }

    /// <summary>
    /// 校验并消费公众号网页登录 state。
    /// </summary>
    private async Task ValidateWechatOfficialAccountStateAsync(string state, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(state))
        {
            throw new InvalidOperationException("微信公众号登录 state 无效或已过期。");
        }

        var removed = await _cacheService.RemoveAsync(BuildWechatOfficialAccountStateCacheKey(state), cancellationToken);
        if (!removed)
        {
            throw new InvalidOperationException("微信公众号登录 state 无效或已过期。");
        }
    }

    /// <summary>
    /// 创建随机公众号网页登录 state。
    /// </summary>
    private static string CreateWechatOfficialAccountState()
    {
        return Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();
    }

    /// <summary>
    /// 生成公众号网页登录 state 的缓存键。
    /// </summary>
    private static string BuildWechatOfficialAccountStateCacheKey(string state)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(state));
        return $"{WechatOfficialAccountStateCacheKeyPrefix}:{Convert.ToHexString(hash).ToLowerInvariant()}";
    }

    /// <summary>
    /// 刷新小程序用户 Token。
    /// </summary>
    public async Task<AuthTokenResDto> RefreshTokenAsync(RefreshTokenReqDto reqDto, CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.ExecuteInTransactionAsync(async tokenCancellationToken =>
        {
            var oldRefreshToken = await _repository.GetRefreshTokenAsync(reqDto.RefreshToken, "client", tokenCancellationToken) ??
                                  throw new InvalidOperationException("刷新 Token 无效或已过期。");

            var token = _tokenService.IssueToken(oldRefreshToken.SubjectId, "client", Array.Empty<string>(), new[]
            {
                new Claim("memberId", oldRefreshToken.SubjectId.ToString())
            });

            await _repository.RevokeRefreshTokenAsync(reqDto.RefreshToken, "client", tokenCancellationToken);
            await _repository.SaveRefreshTokenAsync(CreateRefreshToken(oldRefreshToken.SubjectId, "client", token.RefreshToken), tokenCancellationToken);

            return token;
        }, cancellationToken);
    }

    /// <summary>
    /// 退出小程序登录。
    /// </summary>
    public async Task LogoutAsync(RefreshTokenReqDto reqDto, CancellationToken cancellationToken = default)
    {
        await _repository.RevokeRefreshTokenAsync(reqDto.RefreshToken, "client", cancellationToken);
    }

    /// <summary>
    /// 创建刷新 Token 实体。
    /// </summary>
    private static RefreshToken CreateRefreshToken(Guid subjectId, string scope, string token)
    {
        return new RefreshToken
        {
            Id = Guid.NewGuid(),
            SubjectId = subjectId,
            Scope = scope,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };
    }
}
