using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ocow.Identity.Domain.Models;

/// <summary>
/// 登录日志实体，用于记录管理员和会员登录结果。/// </summary>
[Table("login_logs")]
public class LoginLog
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(128)]
    public string LoginName { get; set; } = string.Empty;

    [Required]
    [MaxLength(32)]
    public string Scope { get; set; } = string.Empty;

    public bool Success { get; set; }

    [MaxLength(256)]
    public string? FailureReason { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
