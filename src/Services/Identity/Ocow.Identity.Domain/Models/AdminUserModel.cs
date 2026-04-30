using Ocow.Identity.Domain.Enums;

namespace Ocow.Identity.Domain.Models;

/// <summary>
/// 后台管理员实体，用于保存管理员账号和密码摘要。
/// </summary>
public class AdminUserModel
{
    public Guid Id { get; set; }

    public string UserName { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public AdminUserStatusEnum Status { get; set; } = AdminUserStatusEnum.Enabled;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
