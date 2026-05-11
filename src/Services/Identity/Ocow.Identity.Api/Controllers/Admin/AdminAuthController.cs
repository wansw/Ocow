using Microsoft.AspNetCore.Mvc;
using Ocow.Identity.Application.Dtos;
using Ocow.Identity.Application.Interfaces;
using Ocow.Shared.Controllers;
using Ocow.Shared.Dtos;
using Ocow.Shared.SwaggerApi;

namespace Ocow.Identity.Api.Controllers.Admin;

/// <summary>
/// 后台认证接口，用于管理员登录、刷。Token 和退出登录。
/// </summary>

[ApiExplorerSettings(GroupName = SwaggerApiGroupNames.Admin)]
[Route("api/admin/auth")]
[Tags("后台认证")]
public class AdminAuthController : BaseController
{
    private readonly IAdminAuthAppService _adminAuthAppService;

    /// <summary>
    /// 创建后台认证 Controller。    
    /// </summary>
    public AdminAuthController(IAdminAuthAppService adminAuthAppService)
    {
        _adminAuthAppService = adminAuthAppService;
    }

    /// <summary>
    /// 管理员登录并签发 Admin JWT。    
    /// </summary>
    [HttpPost("login")]
    public async Task<ApiResDto<AuthTokenResDto>> LoginAsync([FromBody] AdminLoginReqDto reqDto, CancellationToken cancellationToken)
    {
        var token = await _adminAuthAppService.LoginAsync(reqDto, cancellationToken);
        return Success(token);
    }

    /// <summary>
    /// 刷新管理。Token。    
    /// </summary>
    [HttpPost("refresh-token")]
    public async Task<ApiResDto<AuthTokenResDto>> RefreshTokenAsync([FromBody] RefreshTokenReqDto reqDto, CancellationToken cancellationToken)
    {
        var token = await _adminAuthAppService.RefreshTokenAsync(reqDto, cancellationToken);
        return Success(token);
    }

    /// <summary>
    /// 管理员退出登录。    
    /// </summary>
    [HttpPost("logout")]
    public async Task<ApiResDto<bool>> LogoutAsync([FromBody] RefreshTokenReqDto reqDto, CancellationToken cancellationToken)
    {
        await _adminAuthAppService.LogoutAsync(reqDto, cancellationToken);
        return Success();
    }
}
