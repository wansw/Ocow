using Microsoft.AspNetCore.Mvc;
using Ocow.Identity.Application.Dtos;
using Ocow.Identity.Application.Interfaces;
using Ocow.Shared.Dtos;

namespace Ocow.Identity.Api.Controllers.Admin;

/// <summary>
/// 后台认证接口，用于管理员登录、刷新 Token 和退出登录。
/// </summary>
[ApiController]
[Route("api/admin/auth")]
public class AdminAuthController : ControllerBase
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
    public async Task<ActionResult<ApiResDto<AuthTokenResDto>>> LoginAsync([FromBody] AdminLoginReqDto reqDto, CancellationToken cancellationToken)
    {
        var token = await _adminAuthAppService.LoginAsync(reqDto, cancellationToken);
        return ApiResDto<AuthTokenResDto>.Ok(token, HttpContext.TraceIdentifier);
    }

    /// <summary>
    /// 刷新管理员 Token。
    /// </summary>
    [HttpPost("refresh-token")]
    public async Task<ActionResult<ApiResDto<AuthTokenResDto>>> RefreshTokenAsync([FromBody] RefreshTokenReqDto reqDto, CancellationToken cancellationToken)
    {
        var token = await _adminAuthAppService.RefreshTokenAsync(reqDto, cancellationToken);
        return ApiResDto<AuthTokenResDto>.Ok(token, HttpContext.TraceIdentifier);
    }

    /// <summary>
    /// 管理员退出登录。
    /// </summary>
    [HttpPost("logout")]
    public async Task<ActionResult<ApiResDto<bool>>> LogoutAsync([FromBody] RefreshTokenReqDto reqDto, CancellationToken cancellationToken)
    {
        await _adminAuthAppService.LogoutAsync(reqDto, cancellationToken);
        return ApiResDto<bool>.Ok(true, HttpContext.TraceIdentifier);
    }
}
