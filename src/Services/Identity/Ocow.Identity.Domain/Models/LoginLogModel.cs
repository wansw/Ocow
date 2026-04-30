namespace Ocow.Identity.Domain.Models;

/// <summary>
/// 登录日志实体，用于记录管理员和会员登录结果。
/// </summary>
public class LoginLogModel
{
    public Guid Id { get; set; }

    public string LoginName { get; set; } = string.Empty;

    public string Scope { get; set; } = string.Empty;

    public bool Success { get; set; }

    public string? FailureReason { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
