using Microsoft.AspNetCore.Mvc;
using Ocow.Identity.Application.Dtos;
using Ocow.Identity.Application.Interfaces;
using Ocow.Shared.Dtos;

namespace Ocow.Identity.Api.Controllers.Admin;

/// <summary>
/// 后台权限点接口，用于查询系统可授权权限点。
/// </summary>
[ApiController]
[Route("api/admin/permissions")]
public class AdminPermissionsController : ControllerBase
{
    private readonly IRolePermissionAppService _rolePermissionAppService;

    public AdminPermissionsController(IRolePermissionAppService rolePermissionAppService)
    {
        _rolePermissionAppService = rolePermissionAppService;
    }

    /// <summary>
    /// 查询权限点列表。
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResDto<IReadOnlyList<PermissionResDto>>>> GetListAsync(CancellationToken cancellationToken)
    {
        var result = await _rolePermissionAppService.GetPermissionsAsync(cancellationToken);
        return ApiResDto<IReadOnlyList<PermissionResDto>>.Ok(result, HttpContext.TraceIdentifier);
    }
}
