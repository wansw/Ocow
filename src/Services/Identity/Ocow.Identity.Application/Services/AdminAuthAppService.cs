using Ocow.Identity.Application.Dtos;
using Ocow.Identity.Application.Interfaces;
using Ocow.Identity.Domain.Enums;
using Ocow.Identity.Domain.Models;

namespace Ocow.Identity.Application.Services;

/// <summary>
/// 后台认证应用服务，用于管理员登录和 Token 签发。
/// </summary>
public class AdminAuthAppService : IAdminAuthAppService
{
    private readonly IIdentityRepository _repository;
    private readonly ITokenService _tokenService;

    public AdminAuthAppService(IIdentityRepository repository, ITokenService tokenService)
    {
        _repository = repository;
        _tokenService = tokenService;
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

        await _repository.AddLoginLogAsync(new LoginLogModel
        {
            Id = Guid.NewGuid(),
            LoginName = reqDto.UserName,
            Scope = "admin",
            Success = success,
            FailureReason = success ? null : "用户名或密码错误。"
        }, cancellationToken);

        if (!success || adminUser is null)
        {
            throw new InvalidOperationException("用户名或密码错误。");
        }

        var permissions = await _repository.GetAdminPermissionCodesAsync(adminUser.Id, cancellationToken);
        var token = _tokenService.IssueToken(adminUser.Id, "admin", permissions);
        await _repository.SaveRefreshTokenAsync(CreateRefreshToken(adminUser.Id, "admin", token.RefreshToken), cancellationToken);

        return token;
    }

    /// <summary>
    /// 刷新后台管理员 Token。
    /// </summary>
    public async Task<AuthTokenResDto> RefreshTokenAsync(RefreshTokenReqDto reqDto, CancellationToken cancellationToken = default)
    {
        var oldRefreshToken = await _repository.GetRefreshTokenAsync(reqDto.RefreshToken, "admin", cancellationToken) ??
                              throw new InvalidOperationException("刷新 Token 无效或已过期。");

        var permissions = await _repository.GetAdminPermissionCodesAsync(oldRefreshToken.SubjectId, cancellationToken);
        var token = _tokenService.IssueToken(oldRefreshToken.SubjectId, "admin", permissions);

        await _repository.RevokeRefreshTokenAsync(reqDto.RefreshToken, "admin", cancellationToken);
        await _repository.SaveRefreshTokenAsync(CreateRefreshToken(oldRefreshToken.SubjectId, "admin", token.RefreshToken), cancellationToken);

        return token;
    }

    /// <summary>
    /// 退出后台登录。
    /// </summary>
    public async Task LogoutAsync(RefreshTokenReqDto reqDto, CancellationToken cancellationToken = default)
    {
        await _repository.RevokeRefreshTokenAsync(reqDto.RefreshToken, "admin", cancellationToken);
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
