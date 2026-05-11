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
/// 后台角色接口，用于管理 RBAC 角色和角色权限点绑定。
/// </summary>
[ApiExplorerSettings(GroupName = SwaggerApiGroupNames.Admin)]
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
    [PermissionAuthorize("identity.role.read")]
    public async Task<ApiResDto<IReadOnlyList<RoleResDto>>> GetListAsync(CancellationToken cancellationToken)
    {
        var result = await _rolePermissionAppService.GetRolesAsync(cancellationToken);
        return Success(result);
    }

    /// <summary>
    /// 创建角色。
    /// </summary>
    [HttpPost]
    [PermissionAuthorize("identity.role.save")]
    public async Task<ApiResDto<RoleResDto>> CreateAsync([FromBody] RoleReqDto reqDto, CancellationToken cancellationToken)
    {
        var result = await _rolePermissionAppService.SaveRoleAsync(null, reqDto, cancellationToken);
        return Success(result);
    }

    /// <summary>
    /// 修改角色。
    /// </summary>
    [HttpPut("{id:guid}")]
    [PermissionAuthorize("identity.role.save")]
    public async Task<ApiResDto<RoleResDto>> UpdateAsync(Guid id, [FromBody] RoleReqDto reqDto, CancellationToken cancellationToken)
    {
        var result = await _rolePermissionAppService.SaveRoleAsync(id, reqDto, cancellationToken);
        return Success(result);
    }

    /// <summary>
    /// 绑定角色权限点。
    /// </summary>
    [HttpPut("{id:guid}/permissions")]
    [PermissionAuthorize("identity.role.bind-permission")]
    public async Task<ApiResDto<bool>> BindPermissionsAsync(Guid id, [FromBody] BindRolePermissionsReqDto reqDto, CancellationToken cancellationToken)
    {
        await _rolePermissionAppService.BindRolePermissionsAsync(id, reqDto, cancellationToken);
        return Success();
    }
}
