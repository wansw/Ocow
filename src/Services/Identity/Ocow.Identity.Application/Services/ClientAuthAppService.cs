using System.Security.Claims;
using Ocow.Identity.Application.Dtos;
using Ocow.Identity.Application.Interfaces;
using Ocow.Identity.Domain.Models;

namespace Ocow.Identity.Application.Services;

/// <summary>
/// 小程序认证应用服务，用于微信登录和 Customer JWT 签发。
/// </summary>
public class ClientAuthAppService : IClientAuthAppService
{
    private readonly IIdentityRepository _repository;
    private readonly ITokenService _tokenService;

    public ClientAuthAppService(IIdentityRepository repository, ITokenService tokenService)
    {
        _repository = repository;
        _tokenService = tokenService;
    }

    /// <summary>
    /// 小程序微信登录并签发 Customer JWT。
    /// </summary>
    public async Task<AuthTokenResDto> WechatLoginAsync(WechatLoginReqDto reqDto, CancellationToken cancellationToken = default)
    {
        var openId = string.IsNullOrWhiteSpace(reqDto.OpenId) ? $"mock-openid-{reqDto.Code}" : reqDto.OpenId;
        var identity = await _repository.GetMemberIdentityByOpenIdAsync(openId, cancellationToken);

        if (identity is null)
        {
            identity = new MemberIdentityModel
            {
                Id = Guid.NewGuid(),
                MemberId = Guid.NewGuid(),
                OpenId = openId,
                UnionId = reqDto.UnionId
            };
            await _repository.SaveMemberIdentityAsync(identity, cancellationToken);
        }

        var token = _tokenService.IssueToken(identity.MemberId, "client", Array.Empty<string>(), new[]
        {
            new Claim("memberId", identity.MemberId.ToString()),
            new Claim("openid", identity.OpenId)
        });
        await _repository.SaveRefreshTokenAsync(CreateRefreshToken(identity.MemberId, "client", token.RefreshToken), cancellationToken);

        return token;
    }

    /// <summary>
    /// 刷新小程序用户 Token。
    /// </summary>
    public Task<AuthTokenResDto> RefreshTokenAsync(RefreshTokenReqDto reqDto, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("刷新 Token 的持久化校验将在下一阶段接入。");
    }

    /// <summary>
    /// 退出小程序登录。
    /// </summary>
    public Task LogoutAsync(RefreshTokenReqDto reqDto, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// 创建刷新 Token 实体。
    /// </summary>
    private static RefreshTokenModel CreateRefreshToken(Guid subjectId, string scope, string token)
    {
        return new RefreshTokenModel
        {
            Id = Guid.NewGuid(),
            SubjectId = subjectId,
            Scope = scope,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };
    }
}
