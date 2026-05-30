using System.Text.Json;
using Microsoft.Extensions.Options;
using Ocow.WeChat.Abstractions.Dtos;
using Ocow.WeChat.Abstractions.Interfaces;
using Ocow.WeChat.Core.Exceptions;
using Ocow.WeChat.Core.Options;

namespace Ocow.WeChat.Http.Services;

/// <summary>
/// 微信公众号 HTTP 客户端，用于调用公众号网页授权开放接口。
/// </summary>
public class WechatOfficialAccountClient : IWechatOfficialAccountClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly WechatOfficialAccountOption _option;

    /// <summary>
    /// 创建微信公众号 HTTP 客户端。
    /// </summary>
    public WechatOfficialAccountClient(HttpClient httpClient, IOptions<WechatOfficialAccountOption> option)
    {
        _httpClient = httpClient;
        _option = option.Value;
    }

    /// <summary>
    /// 使用公众号网页授权 code 换取微信 openid、unionid 和网页授权凭证。
    /// </summary>
    public async Task<WechatOfficialAccountOAuthResDto> GetOAuthAccessTokenAsync(WechatOfficialAccountOAuthReqDto reqDto, CancellationToken cancellationToken = default)
    {
        var appId = ResolveAppId(reqDto.AppId);
        var appSecret = ResolveAppSecret();
        if (string.IsNullOrWhiteSpace(reqDto.Code))
        {
            throw new ArgumentException("微信公众号授权 code 不能为空。", nameof(reqDto));
        }

        var url = $"sns/oauth2/access_token?appid={Uri.EscapeDataString(appId)}&secret={Uri.EscapeDataString(appSecret)}&code={Uri.EscapeDataString(reqDto.Code)}&grant_type=authorization_code";
        using var response = await _httpClient.GetAsync(url, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = JsonSerializer.Deserialize<WechatOfficialAccountOAuthResDto>(content, JsonOptions)
                     ?? throw new WechatApiException(-1, "微信公众号授权响应为空。");

        ValidateWechatResult(result.ErrCode, result.ErrMsg, result.OpenId, "微信公众号授权结果缺少 openid。");
        return result;
    }

    /// <summary>
    /// 获取请求使用的公众号 AppId。
    /// </summary>
    private string ResolveAppId(string? requestAppId)
    {
        var appId = string.IsNullOrWhiteSpace(requestAppId) ? _option.AppId : requestAppId;
        if (string.IsNullOrWhiteSpace(appId))
        {
            throw new InvalidOperationException("微信公众号 AppId 未配置。");
        }

        return appId;
    }

    /// <summary>
    /// 获取请求使用的公众号 AppSecret。
    /// </summary>
    private string ResolveAppSecret()
    {
        if (string.IsNullOrWhiteSpace(_option.AppSecret))
        {
            throw new InvalidOperationException("微信公众号 AppSecret 未配置。");
        }

        return _option.AppSecret;
    }

    /// <summary>
    /// 校验微信响应中的错误码和 openid。
    /// </summary>
    private static void ValidateWechatResult(int? errCode, string? errMsg, string openId, string missingOpenIdMessage)
    {
        if (errCode.HasValue && errCode.Value != 0)
        {
            throw new WechatApiException(errCode.Value, errMsg);
        }

        if (string.IsNullOrWhiteSpace(openId))
        {
            throw new WechatApiException(-1, missingOpenIdMessage);
        }
    }
}
