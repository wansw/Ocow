namespace Ocow.Identity.Application.Dtos;

/// <summary>
/// 公众号网页登录 state 响应 DTO，用于返回一次性防重放 state 和过期时间。
/// </summary>
public class WechatOfficialAccountStateResDto
{
    /// <summary>
    /// 公众号网页授权 state，客户端需要在微信回调登录时原样回传。
    /// </summary>
    public string State { get; init; } = string.Empty;

    /// <summary>
    /// state 过期时间。
    /// </summary>
    public DateTime ExpiresAt { get; init; }
}
