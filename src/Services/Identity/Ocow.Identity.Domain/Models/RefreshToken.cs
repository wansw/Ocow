using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ocow.Identity.Domain.Models;

/// <summary>
/// 刷新 Token 实体，用于支持访。Token 续签和退出登录。/// </summary>
[Table("refresh_tokens")]
public class RefreshToken
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(256)]
    public string Token { get; set; } = string.Empty;

    [Required]
    [MaxLength(32)]
    public string Scope { get; set; } = string.Empty;

    public Guid SubjectId { get; set; }

    public DateTime ExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? RevokedAt { get; set; }
}
