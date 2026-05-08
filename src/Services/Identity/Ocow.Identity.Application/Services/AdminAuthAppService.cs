using System.IdentityModel.Tokens.Jwt;
using Ocow.EntityFrameworkCore.Abstractions;
using Ocow.Identity.Application.Dtos;
using Ocow.Identity.Application.Interfaces;
using Ocow.Identity.Application.Models;
using Ocow.Identity.Domain.Enums;
using Ocow.Identity.Domain.Models;
using Ocow.InternalAuth.Models;

namespace Ocow.Identity.Application.Services;

/// <summary>
/// 后台认证应用服务，用于管理员登录、刷新 Token 和退出登录。
/// </summary>
public class AdminAuthAppService : IAdminAuthAppService
{
    private const string AdminScope = "admin";

    private readonly IIdentityRepository _repository;
    private readonly ITokenService _tokenService;
    private readonly IRedisTokenSessionService _redisTokenSessionService;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// 创建后台认证应用服务。
    /// </summary>
    public AdminAuthAppService(
        IIdentityRepository repository,
        ITokenService tokenService,
        IRedisTokenSessionService redisTokenSessionService,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _tokenService = tokenService;
        _redisTokenSessionService = redisTokenSessionService;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// 后台管理员登录并签发 Admin JWT。
    /// </summary>
    public async Task<AuthTokenResDto> LoginAsync(AdminLoginReqDto reqDto, CancellationToken cancellationToken = default)
    {
        var adminUser = await _repository.GetAdminUserByNameAsync(reqDto.UserName, cancellationToken);
        var success = adminUser is not null &&
                      adminUser.Status == AdminUserStatusEnum.Enabled &&
                      PasswordHashService.Verify(reqDto.Password, adminUser.PasswordHash);

        var loginLog = new LoginLog
        {
            Id = Guid.NewGuid(),
            LoginName = reqDto.UserName,
            Scope = AdminScope,
            Success = success,
            FailureReason = success ? null : "用户名或密码错误。"
        };

        if (!success || adminUser is null)
        {
            await _repository.AddLoginLogAsync(loginLog, cancellationToken);
            throw new InvalidOperationException("用户名或密码错误。");
        }

        return await _unitOfWork.ExecuteInTransactionAsync(async tokenCancellationToken =>
        {
            await _repository.AddLoginLogAsync(loginLog, tokenCancellationToken);

            var permissions = await _repository.GetAdminPermissionCodesAsync(adminUser.Id, tokenCancellationToken);

            var tokenModel = _tokenService.IssueToken(adminUser.Id, AdminScope, permissions);

            var refreshToken = CreateRefreshToken(adminUser.Id, AdminScope, tokenModel.RefreshToken);

            await _repository.SaveRefreshTokenAsync(refreshToken, tokenCancellationToken);

            await SaveAdminTokenSessionAsync(adminUser.Id, tokenModel, refreshToken.ExpiresAt, tokenCancellationToken);

            return tokenModel;
        }, cancellationToken);
    }

    /// <summary>
    /// 刷新后台管理 Token。
    /// </summary>
    public async Task<AuthTokenResDto> RefreshTokenAsync(RefreshTokenReqDto reqDto, CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.ExecuteInTransactionAsync(async tokenCancellationToken =>
        {
            var oldSession = await _redisTokenSessionService.GetAdminRefreshTokenAsync(reqDto.RefreshToken, tokenCancellationToken);

            var oldRefreshToken = await _repository.GetRefreshTokenAsync(reqDto.RefreshToken, AdminScope, tokenCancellationToken) ??
                                  throw new InvalidOperationException("刷新 Token 无效或已过期。");

            var permissions = await _repository.GetAdminPermissionCodesAsync(oldRefreshToken.SubjectId, tokenCancellationToken);

            var token = _tokenService.IssueToken(oldRefreshToken.SubjectId,AdminScope,permissions);

            var refreshToken = CreateRefreshToken(oldRefreshToken.SubjectId, AdminScope, token.RefreshToken);

            await _repository.RevokeRefreshTokenAsync(reqDto.RefreshToken, AdminScope, tokenCancellationToken);

            await _repository.SaveRefreshTokenAsync(refreshToken, tokenCancellationToken);

            if (oldSession is not null)
            {
                await _redisTokenSessionService.RevokeAdminSessionAsync(oldSession.SessionId, oldSession.JwtId, oldSession.AccessTokenExpiresAt, tokenCancellationToken);
            }

            await _redisTokenSessionService.RemoveAdminRefreshTokenAsync(reqDto.RefreshToken, tokenCancellationToken);
            await SaveAdminTokenSessionAsync(oldRefreshToken.SubjectId, token, refreshToken.ExpiresAt, tokenCancellationToken);

            return token;
        }, cancellationToken);
    }

    /// <summary>
    /// 退出后台登录。
    /// </summary>
    public async Task LogoutAsync(RefreshTokenReqDto reqDto, CancellationToken cancellationToken = default)
    {
        var oldSession = await _redisTokenSessionService.GetAdminRefreshTokenAsync(reqDto.RefreshToken, cancellationToken);
        await _repository.RevokeRefreshTokenAsync(reqDto.RefreshToken, AdminScope, cancellationToken);
        await _redisTokenSessionService.RemoveAdminRefreshTokenAsync(reqDto.RefreshToken, cancellationToken);
        if (oldSession is not null)
        {
            await _redisTokenSessionService.RevokeAdminSessionAsync(oldSession.SessionId, oldSession.JwtId, oldSession.AccessTokenExpiresAt, cancellationToken);
        }
    }

    /// <summary>
    /// 保存 Admin JWT 会话和刷新 Token 会话缓存。
    /// </summary>
    private async Task SaveAdminTokenSessionAsync(
        Guid adminUserId,
        AuthTokenResDto token,
        DateTime refreshTokenExpiresAt,
        CancellationToken cancellationToken)
    {
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token.AccessToken);
        var sessionId = Guid.Parse(jwt.Claims.Single(x => x.Type == "sid").Value);
        var jwtId = jwt.Claims.Single(x => x.Type == JwtRegisteredClaimNames.Jti).Value;

        await _redisTokenSessionService.SaveAdminSessionAsync(new TokenSession
        {
            SessionId = sessionId,
            SubjectId = adminUserId,
            Scope = AdminScope,
            JwtId = jwtId,
            ExpiresAt = token.ExpiresAt
        }, cancellationToken);

        await _redisTokenSessionService.SaveAdminRefreshTokenAsync(token.RefreshToken, new RefreshTokenSession
        {
            SessionId = sessionId,
            SubjectId = adminUserId,
            JwtId = jwtId,
            AccessTokenExpiresAt = token.ExpiresAt,
            RefreshTokenExpiresAt = refreshTokenExpiresAt
        }, cancellationToken);
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
