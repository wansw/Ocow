namespace Ocow.WeChat.Core.Options;

/// <summary>
/// 微信公众号配置实体，用于保存公众号 AppId、密钥和微信 API 地址。
/// </summary>
public class WechatOfficialAccountOption
{
    /// <summary>
    /// 微信公众号 AppId。
    /// </summary>
    public string AppId { get; set; } = string.Empty;

    /// <summary>
    /// 微信公众号 AppSecret。
    /// </summary>
    public string AppSecret { get; set; } = string.Empty;

    /// <summary>
    /// 微信开放接口基础地址。
    /// </summary>
    public string ApiBaseUrl { get; set; } = "https://api.weixin.qq.com";
}
