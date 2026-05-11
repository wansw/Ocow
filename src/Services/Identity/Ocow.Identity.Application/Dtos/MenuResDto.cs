using Ocow.Identity.Domain.Enums;

namespace Ocow.Identity.Application.Dtos;

/// <summary>
/// 菜单响应 DTO，用于返回后台页面和按钮菜单树。
/// </summary>
public class MenuResDto
{
    /// <summary>
    /// 菜单编号。
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// 父级菜单编号。
    /// </summary>
    public Guid? ParentId { get; init; }

    /// <summary>
    /// 菜单编码。
    /// </summary>
    public string Code { get; init; } = string.Empty;

    /// <summary>
    /// 菜单名称。
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// 菜单类型。
    /// </summary>
    public MenuTypeEnum Type { get; init; }

    /// <summary>
    /// 前端路由路径。
    /// </summary>
    public string? Path { get; init; }

    /// <summary>
    /// 前端组件路径。
    /// </summary>
    public string? Component { get; init; }

    /// <summary>
    /// 菜单图标。
    /// </summary>
    public string? Icon { get; init; }

    /// <summary>
    /// 排序值。
    /// </summary>
    public int Sort { get; init; }

    /// <summary>
    /// 绑定的权限点编号。
    /// </summary>
    public Guid? PermissionId { get; init; }

    /// <summary>
    /// 绑定的权限点编码。
    /// </summary>
    public string? PermissionCode { get; init; }

    /// <summary>
    /// 是否在菜单树中显示。
    /// </summary>
    public bool IsVisible { get; init; }

    /// <summary>
    /// 是否启用。
    /// </summary>
    public bool IsEnabled { get; init; }

    /// <summary>
    /// 子级菜单集合。
    /// </summary>
    public List<MenuResDto> Children { get; init; } = new();
}
