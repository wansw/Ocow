using System.Text.Json.Serialization;

namespace Ocow.WeChat.Abstractions.Dtos;

/// <summary>
/// 公众号网页授权响应 DTO，用于承载 openid、网页授权 access_token 和 unionid。
/// </summary>
public class WechatOfficialAccountOAuthResDto
{
    /// <summary>
    /// 网页授权接口调用凭证。
    /// </summary>
    [JsonPropertyName("access_token")]
    public string? AccessToken { get; init; }

    /// <summary>
    /// 网页授权接口调用凭证有效秒数。
    /// </summary>
    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; init; }

    /// <summary>
    /// 网页授权刷新凭证。
    /// </summary>
    [JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; init; }

    /// <summary>
    /// 用户在当前公众号下的唯一标识。
    /// </summary>
    [JsonPropertyName("openid")]
    public string OpenId { get; init; } = string.Empty;

    /// <summary>
    /// 用户授权作用域。
    /// </summary>
    [JsonPropertyName("scope")]
    public string? Scope { get; init; }

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
