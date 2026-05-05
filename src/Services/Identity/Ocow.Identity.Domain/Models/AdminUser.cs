using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Ocow.Identity.Domain.Enums;

namespace Ocow.Identity.Domain.Models;

/// <summary>
/// 后台管理员实体，用于保存管理员账号和密码摘要。/// </summary>
[Table("admin_users")]
public class AdminUser
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(64)]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [MaxLength(128)]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    [MaxLength(64)]
    public string DisplayName { get; set; } = string.Empty;

    public AdminUserStatusEnum Status { get; set; } = AdminUserStatusEnum.Enabled;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 管理员角色绑定集合，用于表达管理员与角色的多对多关系。    /// </summary>
    [InverseProperty(nameof(AdminUserRole.AdminUser))]
    public List<AdminUserRole> AdminUserRoles { get; set; } = new();
}
