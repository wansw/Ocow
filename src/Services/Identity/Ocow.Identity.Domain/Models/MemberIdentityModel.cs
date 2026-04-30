namespace Ocow.Identity.Domain.Models;

/// <summary>
/// 会员身份实体，用于保存小程序 openid 与会员编号的绑定关系。
/// </summary>
public class MemberIdentityModel
{
    public Guid Id { get; set; }

    public Guid MemberId { get; set; }

    public string OpenId { get; set; } = string.Empty;

    public string? UnionId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
