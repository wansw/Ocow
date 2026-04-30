namespace Ocow.Identity.Domain.Models;

/// <summary>
/// 刷新 Token 实体，用于支持访问 Token 续签和退出登录。
/// </summary>
public class RefreshTokenModel
{
    public Guid Id { get; set; }

    public string Token { get; set; } = string.Empty;

    public string Scope { get; set; } = string.Empty;

    public Guid SubjectId { get; set; }

    public DateTime ExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? RevokedAt { get; set; }
}
