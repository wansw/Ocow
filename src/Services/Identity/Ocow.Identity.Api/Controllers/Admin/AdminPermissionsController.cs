using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ocow.Identity.Application.Dtos;
using Ocow.Identity.Application.Interfaces;
using Ocow.Auth.Attributes;
using Ocow.Auth.Extensions;
using Ocow.AspNetCore.Controllers;
using Ocow.Shared.Dtos;

namespace Ocow.Identity.Api.Controllers.Admin;

/// <summary>
/// 后台权限点接口，用于查询系统可授权权限点。
/// </summary>
[Tags("后台权限点")]
[Route("api/admin/permissions")]
public class AdminPermissionsController : AdminController
{
    private readonly IRolePermissionAppService _rolePermissionAppService;

    /// <summary>
    /// 创建后台权限点 Controller。
    /// </summary>
    public AdminPermissionsController(IRolePermissionAppService rolePermissionAppService)
    {
        _rolePermissionAppService = rolePermissionAppService;
    }

    /// <summary>
    /// 查询权限点列表。
    /// </summary>
    [HttpGet]
    [PermissionAuthorize("identity.permission.read")]
    public async Task<ApiResDto<IReadOnlyList<PermissionResDto>>> GetListAsync(CancellationToken cancellationToken)
    {
        var result = await _rolePermissionAppService.GetPermissionsAsync(cancellationToken);
        return Success(result);
    }
}
