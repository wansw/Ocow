using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ocow.InternalAuth.Extensions;
using Ocow.Identity.Application.Dtos;
using Ocow.Identity.Application.Interfaces;
using Ocow.Shared.Controllers;
using Ocow.Shared.Dtos;
using Ocow.Shared.OpenApi;

namespace Ocow.Identity.Api.Controllers.Admin;

/// <summary>
/// 后台管理员接口，用于管理管理员账号。
///  </summary>

[ApiExplorerSettings(GroupName = OpenApiGroupNames.Admin)]
[Route("api/admin/users")]
[Authorize(Policy = InternalAuthServiceCollectionExtensions.AdminOnlyPolicy)]
[Tags("后台管理员")]
public class AdminUsersController : BaseController
{
    private readonly IAdminUserAppService _adminUserAppService;

    /// <summary>
    /// 创建后台管理。Controller。    
    ///  </summary>
    public AdminUsersController(IAdminUserAppService adminUserAppService)
    {
        _adminUserAppService = adminUserAppService;
    }

    /// <summary>
    /// 查询管理员列表。
    /// </summary>
    [HttpGet]
    public async Task<ApiResDto<PageResDto<AdminUserResDto>>> GetListAsync([FromQuery] PageReqDto reqDto, CancellationToken cancellationToken)
    {
        var result = await _adminUserAppService.GetListAsync(reqDto, cancellationToken);
        return Success(result);
    }

    /// <summary>
    /// 创建管理员账号。    
    /// </summary>
    [HttpPost]
    public async Task<ApiResDto<AdminUserResDto>> CreateAsync([FromBody] AdminUserReqDto reqDto, CancellationToken cancellationToken)
    {
        var result = await _adminUserAppService.CreateAsync(reqDto, cancellationToken);
        return Success(result);
    }

    /// <summary>
    /// 禁用管理员账号。  
    /// </summary>
    [HttpPost("{id:guid}/disable")]
    public async Task<ApiResDto<bool>> DisableAsync(Guid id, CancellationToken cancellationToken)
    {
        await _adminUserAppService.DisableAsync(id, cancellationToken);
        return Success();
    }
}
