using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ocow.InternalAuth.Extensions;
using Ocow.Identity.Application.Dtos;
using Ocow.Identity.Application.Interfaces;
using Ocow.Shared.Controllers;
using Ocow.Shared.Dtos;

namespace Ocow.Identity.Api.Controllers.Admin;

/// <summary>
/// 后台权限点接口，用于查询系统可授权权限点。/// </summary>

[Route("api/admin/permissions")]
[Authorize(Policy = InternalAuthServiceCollectionExtensions.AdminOnlyPolicy)]
[Tags("后台权限点")]
public class AdminPermissionsController : BaseController
{
    private readonly IRolePermissionAppService _rolePermissionAppService;

    /// <summary>
    /// 创建后台权限。Controller。    /// </summary>
    public AdminPermissionsController(IRolePermissionAppService rolePermissionAppService)
    {
        _rolePermissionAppService = rolePermissionAppService;
    }

    /// <summary>
    /// 查询权限点列表。    /// </summary>
    [HttpGet]
    public async Task<ApiResDto<IReadOnlyList<PermissionResDto>>> GetListAsync(CancellationToken cancellationToken)
    {
        var result = await _rolePermissionAppService.GetPermissionsAsync(cancellationToken);
        return Success(result);
    }
}
