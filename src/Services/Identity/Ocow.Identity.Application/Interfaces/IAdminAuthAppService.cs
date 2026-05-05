using Ocow.Identity.Application.Dtos;

namespace Ocow.Identity.Application.Interfaces;

/// <summary>
/// 后台认证应用服务接口，用于管理员登录、刷新和退出。/// </summary>
public interface IAdminAuthAppService
{
    /// <summary>
    /// 后台管理员登录并签发 Admin JWT。    /// </summary>
    Task<AuthTokenResDto> LoginAsync(AdminLoginReqDto reqDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// 刷新后台管理。Token。    /// </summary>
    Task<AuthTokenResDto> RefreshTokenAsync(RefreshTokenReqDto reqDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// 退出后台登录。    /// </summary>
    Task LogoutAsync(RefreshTokenReqDto reqDto, CancellationToken cancellationToken = default);
}
