using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ocow.InternalAuth.Extensions;
using Ocow.Identity.Application.Dtos;
using Ocow.Identity.Application.Interfaces;
using Ocow.Shared.Dtos;

namespace Ocow.Identity.Api.Controllers.Admin;

/// <summary>
/// 后台管理员接口，用于管理管理员账号。/// </summary>
[ApiController]
[Route("api/admin/users")]
[Authorize(Policy = InternalAuthServiceCollectionExtensions.AdminOnlyPolicy)]
public class AdminUsersController : ControllerBase
{
    private readonly IAdminUserAppService _adminUserAppService;

    /// <summary>
    /// 创建后台管理。Controller。    /// </summary>
    public AdminUsersController(IAdminUserAppService adminUserAppService)
    {
        _adminUserAppService = adminUserAppService;
    }

    /// <summary>
    /// 查询管理员列表。    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResDto<PageResDto<AdminUserResDto>>>> GetListAsync([FromQuery] PageReqDto reqDto, CancellationToken cancellationToken)
    {
        var result = await _adminUserAppService.GetListAsync(reqDto, cancellationToken);
        return ApiResDto<PageResDto<AdminUserResDto>>.Ok(result, HttpContext.TraceIdentifier);
    }

    /// <summary>
    /// 创建管理员账号。    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResDto<AdminUserResDto>>> CreateAsync([FromBody] AdminUserReqDto reqDto, CancellationToken cancellationToken)
    {
        var result = await _adminUserAppService.CreateAsync(reqDto, cancellationToken);
        return ApiResDto<AdminUserResDto>.Ok(result, HttpContext.TraceIdentifier);
    }

    /// <summary>
    /// 禁用管理员账号。    /// </summary>
    [HttpPost("{id:guid}/disable")]
    public async Task<ActionResult<ApiResDto<bool>>> DisableAsync(Guid id, CancellationToken cancellationToken)
    {
        await _adminUserAppService.DisableAsync(id, cancellationToken);
        return ApiResDto<bool>.Ok(true, HttpContext.TraceIdentifier);
    }
}
