using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ocow.Identity.Application.Dtos;
using Ocow.Identity.Application.Interfaces;
using Ocow.Auth.Extensions;
using Ocow.AspNetCore.Controllers;
using Ocow.Shared.Dtos;

namespace Ocow.Identity.Api.Controllers.Admin;

/// <summary>
/// 后台个人接口，用于查询当前登录管理员的菜单和权限相关信息。
/// </summary>
[Route("api/admin/profile")]
[Tags("后台个人信息")]
public class AdminProfileController : AdminController
{
    private readonly IRolePermissionAppService _rolePermissionAppService;

    /// <summary>
    /// 创建后台个人信息 Controller。
    /// </summary>
    public AdminProfileController(IRolePermissionAppService rolePermissionAppService)
    {
        _rolePermissionAppService = rolePermissionAppService;
    }

    /// <summary>
    /// 查询当前管理员可见的后台菜单树。
    /// </summary>
    [HttpGet("menus")]
    public async Task<ApiResDto<IReadOnlyList<MenuResDto>>> GetMenusAsync(CancellationToken cancellationToken)
    {
        var adminUserId = GetCurrentAdminUserId();
        var result = await _rolePermissionAppService.GetAdminMenusAsync(adminUserId, cancellationToken);
        return Success(result);
    }

    /// <summary>
    /// 从当前 JWT 中读取管理员编号。
    /// </summary>
    private Guid GetCurrentAdminUserId()
    {
        var subject = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (!Guid.TryParse(subject, out var adminUserId))
        {
            throw new InvalidOperationException("当前管理员身份无效。");
        }

        return adminUserId;
    }
}
