namespace Ocow.Identity.Application.Dtos;

/// <summary>
/// 小程序当前身份响。DTO。/// </summary>
public class ClientProfileResDto
{
    /// <summary>
    /// 会员编号。    /// </summary>
    public Guid MemberId { get; init; }

    /// <summary>
    /// 微信 openid。    /// </summary>
    public string OpenId { get; init; } = string.Empty;
}
