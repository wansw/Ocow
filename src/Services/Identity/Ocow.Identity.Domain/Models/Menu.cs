using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Ocow.Identity.Domain.Enums;

namespace Ocow.Identity.Domain.Models;

/// <summary>
/// 菜单实体，用于表达 PC 后台页面菜单和按钮权限入口。
/// </summary>
[Table("menus")]
public class Menu
{
    [Key]
    public Guid Id { get; set; }

    public Guid? ParentId { get; set; }

    [Required]
    [MaxLength(64)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(64)]
    public string Name { get; set; } = string.Empty;

    public MenuTypeEnum Type { get; set; } = MenuTypeEnum.Page;

    [MaxLength(256)]
    public string? Path { get; set; }

    [MaxLength(256)]
    public string? Component { get; set; }

    [MaxLength(64)]
    public string? Icon { get; set; }

    public int Sort { get; set; }

    public Guid? PermissionId { get; set; }

    public bool IsVisible { get; set; } = true;

    public bool IsEnabled { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 父级菜单，用于表达菜单树层级关系。
    /// </summary>
    [ForeignKey(nameof(ParentId))]
    [InverseProperty(nameof(Children))]
    public Menu? Parent { get; set; }

    /// <summary>
    /// 子级菜单集合，用于表达菜单树层级关系。
    /// </summary>
    [InverseProperty(nameof(Parent))]
    public List<Menu> Children { get; set; } = new();

    /// <summary>
    /// 绑定的权限点，用于根据用户拥有的接口权限控制菜单和按钮可见性。
    /// </summary>
    [ForeignKey(nameof(PermissionId))]
    public Permission? Permission { get; set; }
}
