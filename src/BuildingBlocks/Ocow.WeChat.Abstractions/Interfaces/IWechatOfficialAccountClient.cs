using Ocow.WeChat.Abstractions.Dtos;

namespace Ocow.WeChat.Abstractions.Interfaces;

/// <summary>
/// 微信公众号客户端接口，用于调用公众号网页授权等微信开放接口。
/// </summary>
public interface IWechatOfficialAccountClient
{
    /// <summary>
    /// 使用公众号网页授权 code 换取微信 openid、unionid 和网页授权凭证。
    /// </summary>
    Task<WechatOfficialAccountOAuthResDto> GetOAuthAccessTokenAsync(WechatOfficialAccountOAuthReqDto reqDto, CancellationToken cancellationToken = default);
}
