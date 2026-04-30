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

    /// <summary>
    /// 创建小程序认证应用服务。
    /// </summary>
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
    public async Task<AuthTokenResDto> RefreshTokenAsync(RefreshTokenReqDto reqDto, CancellationToken cancellationToken = default)
    {
        var oldRefreshToken = await _repository.GetRefreshTokenAsync(reqDto.RefreshToken, "client", cancellationToken) ??
                              throw new InvalidOperationException("刷新 Token 无效或已过期。");

        var token = _tokenService.IssueToken(oldRefreshToken.SubjectId, "client", Array.Empty<string>(), new[]
        {
            new Claim("memberId", oldRefreshToken.SubjectId.ToString())
        });

        await _repository.RevokeRefreshTokenAsync(reqDto.RefreshToken, "client", cancellationToken);
        await _repository.SaveRefreshTokenAsync(CreateRefreshToken(oldRefreshToken.SubjectId, "client", token.RefreshToken), cancellationToken);

        return token;
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
