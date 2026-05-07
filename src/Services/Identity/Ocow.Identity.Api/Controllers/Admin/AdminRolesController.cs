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
/// 后台角色接口，用于管。RBAC 角色
/// </summary>

[ApiExplorerSettings(GroupName = OpenApiGroupNames.Admin)]
[Route("api/admin/roles")]
[Authorize(Policy = InternalAuthServiceCollectionExtensions.AdminOnlyPolicy)]
[Tags("后台角色")]
public class AdminRolesController : BaseController
{
    private readonly IRolePermissionAppService _rolePermissionAppService;

    /// <summary>
    /// 创建后台角色 Controller。    
    /// </summary>
    public AdminRolesController(IRolePermissionAppService rolePermissionAppService)
    {
        _rolePermissionAppService = rolePermissionAppService;
    }

    /// <summary>
    /// 查询角色列表。    
    /// </summary>
    [HttpGet]
    public async Task<ApiResDto<IReadOnlyList<RoleResDto>>> GetListAsync(CancellationToken cancellationToken)
    {
        var result = await _rolePermissionAppService.GetRolesAsync(cancellationToken);
        return Success(result);
    }

    /// <summary>
    /// 创建角色。    
    /// </summary>
    [HttpPost]
    public async Task<ApiResDto<RoleResDto>> CreateAsync([FromBody] RoleReqDto reqDto, CancellationToken cancellationToken)
    {
        var result = await _rolePermissionAppService.SaveRoleAsync(null, reqDto, cancellationToken);
        return Success(result);
    }

    /// <summary>
    /// 修改角色。    
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<ApiResDto<RoleResDto>> UpdateAsync(Guid id, [FromBody] RoleReqDto reqDto, CancellationToken cancellationToken)
    {
        var result = await _rolePermissionAppService.SaveRoleAsync(id, reqDto, cancellationToken);
        return Success(result);
    }

    /// <summary>
    /// 绑定角色权限点。    
    /// </summary>
    [HttpPut("{id:guid}/permissions")]
    public async Task<ApiResDto<bool>> BindPermissionsAsync(Guid id, [FromBody] BindRolePermissionsReqDto reqDto, CancellationToken cancellationToken)
    {
        await _rolePermissionAppService.BindRolePermissionsAsync(id, reqDto, cancellationToken);
        return Success();
    }
}
