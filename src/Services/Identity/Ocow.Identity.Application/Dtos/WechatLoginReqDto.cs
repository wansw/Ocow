namespace Ocow.Identity.Application.Dtos;

/// <summary>
/// 小程序微信登录请求 DTO。
/// </summary>
public class WechatLoginReqDto
{
    /// <summary>
    /// 微信小程序登录 code。
    /// </summary>
    public string Code { get; init; } = string.Empty;

    /// <summary>
    /// 调试或微信服务返回的 openid。
    /// </summary>
    public string? OpenId { get; init; }

    /// <summary>
    /// 微信 unionid。
    /// </summary>
    public string? UnionId { get; init; }
}
