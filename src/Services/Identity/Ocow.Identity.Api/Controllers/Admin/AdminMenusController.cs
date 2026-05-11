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
/// 后台菜单接口，用于管理 PC 后台页面菜单和按钮权限入口。
/// </summary>
[ApiExplorerSettings(GroupName = SwaggerApiGroupNames.Admin)]
[Route("api/admin/menus")]
[Authorize(Policy = InternalAuthServiceCollectionExtensions.AdminOnlyPolicy)]
[Tags("后台菜单")]
public class AdminMenusController : BaseController
{
    private readonly IRolePermissionAppService _rolePermissionAppService;

    /// <summary>
    /// 创建后台菜单 Controller。
    /// </summary>
    public AdminMenusController(IRolePermissionAppService rolePermissionAppService)
    {
        _rolePermissionAppService = rolePermissionAppService;
    }

    /// <summary>
    /// 查询后台菜单树。
    /// </summary>
    [HttpGet]
    [PermissionAuthorize("identity.menu.read")]
    public async Task<ApiResDto<IReadOnlyList<MenuResDto>>> GetListAsync(CancellationToken cancellationToken)
    {
        var result = await _rolePermissionAppService.GetMenusAsync(cancellationToken);
        return Success(result);
    }

    /// <summary>
    /// 创建后台菜单。
    /// </summary>
    [HttpPost]
    [PermissionAuthorize("identity.menu.save")]
    public async Task<ApiResDto<MenuResDto>> CreateAsync([FromBody] MenuReqDto reqDto, CancellationToken cancellationToken)
    {
        var result = await _rolePermissionAppService.SaveMenuAsync(null, reqDto, cancellationToken);
        return Success(result);
    }

    /// <summary>
    /// 修改后台菜单。
    /// </summary>
    [HttpPut("{id:guid}")]
    [PermissionAuthorize("identity.menu.save")]
    public async Task<ApiResDto<MenuResDto>> UpdateAsync(Guid id, [FromBody] MenuReqDto reqDto, CancellationToken cancellationToken)
    {
        var result = await _rolePermissionAppService.SaveMenuAsync(id, reqDto, cancellationToken);
        return Success(result);
    }
}
