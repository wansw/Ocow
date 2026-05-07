using Ocow.EntityFrameworkCore.Abstractions;
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
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// 创建后台认证应用服务。
    /// </summary>
    public AdminAuthAppService(IIdentityRepository repository, ITokenService tokenService, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _tokenService = tokenService;
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
            Scope = "admin",
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
            var token = _tokenService.IssueToken(adminUser.Id, "admin", permissions);
            await _repository.SaveRefreshTokenAsync(CreateRefreshToken(adminUser.Id, "admin", token.RefreshToken), tokenCancellationToken);

            return token;
        }, cancellationToken);
    }

    /// <summary>
    /// 刷新后台管理 Token。
    /// </summary>
    public async Task<AuthTokenResDto> RefreshTokenAsync(RefreshTokenReqDto reqDto, CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.ExecuteInTransactionAsync(async tokenCancellationToken =>
        {
            var oldRefreshToken = await _repository.GetRefreshTokenAsync(reqDto.RefreshToken, "admin", tokenCancellationToken) ??
                                  throw new InvalidOperationException("刷新 Token 无效或已过期。");

            var permissions = await _repository.GetAdminPermissionCodesAsync(oldRefreshToken.SubjectId, tokenCancellationToken);
            var token = _tokenService.IssueToken(oldRefreshToken.SubjectId, "admin", permissions);

            await _repository.RevokeRefreshTokenAsync(reqDto.RefreshToken, "admin", tokenCancellationToken);
            await _repository.SaveRefreshTokenAsync(CreateRefreshToken(oldRefreshToken.SubjectId, "admin", token.RefreshToken), tokenCancellationToken);

            return token;
        }, cancellationToken);
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
