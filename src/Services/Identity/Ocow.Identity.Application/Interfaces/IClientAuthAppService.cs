using Ocow.Identity.Application.Dtos;

namespace Ocow.Identity.Application.Interfaces;

/// <summary>
/// 小程序认证应用服务接口，用于微信登录、刷新和当前身份查询。/// </summary>
public interface IClientAuthAppService
{
    /// <summary>
    /// 小程序微信登录并签发 Customer JWT。    /// </summary>
    Task<AuthTokenResDto> WechatLoginAsync(WechatLoginReqDto reqDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// 创建公众号网页登录一次性 state。
    /// </summary>
    Task<WechatOfficialAccountStateResDto> CreateWechatOfficialAccountLoginStateAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 公众号微信登录并签发 Customer JWT。    /// </summary>
    Task<AuthTokenResDto> WechatOfficialAccountLoginAsync(WechatOfficialAccountLoginReqDto reqDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// 刷新小程序用。Token。    /// </summary>
    Task<AuthTokenResDto> RefreshTokenAsync(RefreshTokenReqDto reqDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// 退出小程序登录。    /// </summary>
    Task LogoutAsync(RefreshTokenReqDto reqDto, CancellationToken cancellationToken = default);
}
