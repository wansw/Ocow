namespace Ocow.WeChat.Core.Options;

/// <summary>
/// 微信小程序配置实体，用于保存小程序 AppId、密钥和微信 API 地址。
/// </summary>
public class WechatMiniProgramOption
{
    /// <summary>
    /// 微信小程序 AppId。
    /// </summary>
    public string AppId { get; set; } = string.Empty;

    /// <summary>
    /// 微信小程序 AppSecret。
    /// </summary>
    public string AppSecret { get; set; } = string.Empty;

    /// <summary>
    /// 微信开放接口基础地址。
    /// </summary>
    public string ApiBaseUrl { get; set; } = "https://api.weixin.qq.com";
}
