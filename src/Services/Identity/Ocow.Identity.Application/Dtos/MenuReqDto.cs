using Ocow.Identity.Domain.Enums;

namespace Ocow.Identity.Application.Dtos;

/// <summary>
/// 菜单保存请求 DTO，用于新增或修改后台页面和按钮菜单。
/// </summary>
public class MenuReqDto
{
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
    public MenuTypeEnum Type { get; init; } = MenuTypeEnum.Page;

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
    /// 按钮菜单绑定的权限点编号。
    /// </summary>
    public Guid? PermissionId { get; init; }

    /// <summary>
    /// 是否在菜单树中显示。
    /// </summary>
    public bool IsVisible { get; init; } = true;

    /// <summary>
    /// 是否启用。
    /// </summary>
    public bool IsEnabled { get; init; } = true;
}
