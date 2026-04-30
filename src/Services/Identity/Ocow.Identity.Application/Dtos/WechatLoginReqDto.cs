namespace Ocow.Identity.Application.Dtos;

/// <summary>
/// 小程序微信登录请求 DTO。
/// </summary>
public class WechatLoginReqDto
{
    public string Code { get; init; } = string.Empty;

    public string? OpenId { get; init; }

    public string? UnionId { get; init; }
}
