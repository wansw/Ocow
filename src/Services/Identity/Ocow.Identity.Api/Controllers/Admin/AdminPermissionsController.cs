using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ocow.Identity.Application.Dtos;
using Ocow.Identity.Application.Interfaces;
using Ocow.InternalAuth.Attributes;
using Ocow.InternalAuth.Extensions;
using Ocow.Shared.Controllers;
using Ocow.Shared.Dtos;
using Ocow.Shared.SwaggerApi;

namespace Ocow.Identity.Api.Controllers.Admin;

/// <summary>
/// 后台权限点接口，用于查询系统可授权权限点。
/// </summary>
[ApiExplorerSettings(GroupName = SwaggerApiGroupNames.Admin)]
[Tags("后台权限点")]
[Route("api/admin/permissions")]
[Authorize(Policy = InternalAuthServiceCollectionExtensions.AdminOnlyPolicy)]
public class AdminPermissionsController : BaseController
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
