using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ocow.Identity.Domain.Models;

/// <summary>
/// 权限点实体，用于表达后台可授权的操作点。/// </summary>
[Table("permissions")]
public class Permission
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(64)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(64)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(64)]
    public string Module { get; set; } = string.Empty;

    /// <summary>
    /// 角色权限绑定集合，用于表达权限点与角色的多对多关系。    /// </summary>
    [InverseProperty(nameof(RolePermission.Permission))]
    public List<RolePermission> RolePermissions { get; set; } = new();
}
