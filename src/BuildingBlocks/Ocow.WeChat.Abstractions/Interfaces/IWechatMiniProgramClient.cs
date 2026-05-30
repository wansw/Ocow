using Ocow.WeChat.Abstractions.Dtos;

namespace Ocow.WeChat.Abstractions.Interfaces;

/// <summary>
/// 微信小程序客户端接口，用于调用小程序登录等微信开放接口。
/// </summary>
public interface IWechatMiniProgramClient
{
    /// <summary>
    /// 使用小程序登录 code 换取微信 openid、unionid 和 session_key。
    /// </summary>
    Task<WechatCode2SessionResDto> Code2SessionAsync(WechatCode2SessionReqDto reqDto, CancellationToken cancellationToken = default);
}
