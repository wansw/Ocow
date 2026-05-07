using Microsoft.AspNetCore.Mvc;
using Ocow.Identity.Application.Dtos;
using Ocow.Identity.Application.Interfaces;
using Ocow.Shared.Controllers;
using Ocow.Shared.Dtos;
using Ocow.Shared.OpenApi;

namespace Ocow.Identity.Api.Controllers.Client;

/// <summary>
/// 小程序认证接口，用于微信登录、刷。Token 和退出登录。/// </summary>

[ApiExplorerSettings(GroupName = OpenApiGroupNames.Client)]
[Route("api/auth")]
[Tags("小程序认证")]
public class ClientAuthController : BaseController
{
    private readonly IClientAuthAppService _clientAuthAppService;

    /// <summary>
    /// 创建小程序认。Controller。    /// </summary>
    public ClientAuthController(IClientAuthAppService clientAuthAppService)
    {
        _clientAuthAppService = clientAuthAppService;
    }

    /// <summary>
    /// 小程序微信登录并签发 Customer JWT。    /// </summary>
    [HttpPost("wechat-login")]
    public async Task<ApiResDto<AuthTokenResDto>> WechatLoginAsync([FromBody] WechatLoginReqDto reqDto, CancellationToken cancellationToken)
    {
        var token = await _clientAuthAppService.WechatLoginAsync(reqDto, cancellationToken);
        return Success(token);
    }

    /// <summary>
    /// 刷新小程。Token。    /// </summary>
    [HttpPost("refresh-token")]
    public async Task<ApiResDto<AuthTokenResDto>> RefreshTokenAsync([FromBody] RefreshTokenReqDto reqDto, CancellationToken cancellationToken)
    {
        var token = await _clientAuthAppService.RefreshTokenAsync(reqDto, cancellationToken);
        return Success(token);
    }

    /// <summary>
    /// 小程序退出登录。    /// </summary>
    [HttpPost("logout")]
    public async Task<ApiResDto<bool>> LogoutAsync([FromBody] RefreshTokenReqDto reqDto, CancellationToken cancellationToken)
    {
        await _clientAuthAppService.LogoutAsync(reqDto, cancellationToken);
        return Success();
    }
}
