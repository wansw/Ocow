using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ocow.Identity.Domain.Models;

/// <summary>
/// 会员身份实体，用于保存小程序 openid 与会员编号的绑定关系。
/// </summary>
[Table("member_identities")]
public class MemberIdentityModel
{
    [Key]
    public Guid Id { get; set; }

    public Guid MemberId { get; set; }

    [Required]
    [MaxLength(128)]
    public string OpenId { get; set; } = string.Empty;

    [MaxLength(128)]
    public string? UnionId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
