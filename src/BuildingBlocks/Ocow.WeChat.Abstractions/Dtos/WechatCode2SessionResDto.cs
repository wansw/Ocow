using System.Text.Json.Serialization;

namespace Ocow.WeChat.Abstractions.Dtos;

/// <summary>
/// 小程序登录凭证校验响应 DTO，用于承载微信返回的 openid、session_key 和 unionid。
/// </summary>
public class WechatCode2SessionResDto
{
    /// <summary>
    /// 用户在当前小程序下的唯一标识。
    /// </summary>
    [JsonPropertyName("openid")]
    public string OpenId { get; init; } = string.Empty;

    /// <summary>
    /// 小程序会话密钥，仅允许服务端保存和使用。
    /// </summary>
    [JsonPropertyName("session_key")]
    public string? SessionKey { get; init; }

    /// <summary>
    /// 用户在微信开放平台下的统一标识，满足微信返回条件时才有值。
    /// </summary>
    [JsonPropertyName("unionid")]
    public string? UnionId { get; init; }

    /// <summary>
    /// 微信错误码，成功时通常为空或 0。
    /// </summary>
    [JsonPropertyName("errcode")]
    public int? ErrCode { get; init; }

    /// <summary>
    /// 微信错误说明。
    /// </summary>
    [JsonPropertyName("errmsg")]
    public string? ErrMsg { get; init; }
}
