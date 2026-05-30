using System.Text.Json;
using Microsoft.Extensions.Options;
using Ocow.WeChat.Abstractions.Dtos;
using Ocow.WeChat.Abstractions.Interfaces;
using Ocow.WeChat.Core.Exceptions;
using Ocow.WeChat.Core.Options;

namespace Ocow.WeChat.Http.Services;

/// <summary>
/// 微信小程序 HTTP 客户端，用于调用微信小程序开放接口。
/// </summary>
public class WechatMiniProgramClient : IWechatMiniProgramClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly WechatMiniProgramOption _option;

    /// <summary>
    /// 创建微信小程序 HTTP 客户端。
    /// </summary>
    public WechatMiniProgramClient(HttpClient httpClient, IOptions<WechatMiniProgramOption> option)
    {
        _httpClient = httpClient;
        _option = option.Value;
    }

    /// <summary>
    /// 使用小程序登录 code 换取微信 openid、unionid 和 session_key。
    /// </summary>
    public async Task<WechatCode2SessionResDto> Code2SessionAsync(WechatCode2SessionReqDto reqDto, CancellationToken cancellationToken = default)
    {
        var appId = ResolveAppId(reqDto.AppId);
        var appSecret = ResolveAppSecret();
        if (string.IsNullOrWhiteSpace(reqDto.Code))
        {
            throw new ArgumentException("微信小程序登录 code 不能为空。", nameof(reqDto));
        }

        var url = $"sns/jscode2session?appid={Uri.EscapeDataString(appId)}&secret={Uri.EscapeDataString(appSecret)}&js_code={Uri.EscapeDataString(reqDto.Code)}&grant_type=authorization_code";
        using var response = await _httpClient.GetAsync(url, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = JsonSerializer.Deserialize<WechatCode2SessionResDto>(content, JsonOptions)
                     ?? throw new WechatApiException(-1, "微信小程序登录响应为空。");

        ValidateWechatResult(result.ErrCode, result.ErrMsg, result.OpenId, "微信小程序登录结果缺少 openid。");
        return result;
    }

    /// <summary>
    /// 获取请求使用的小程序 AppId。
    /// </summary>
    private string ResolveAppId(string? requestAppId)
    {
        var appId = string.IsNullOrWhiteSpace(requestAppId) ? _option.AppId : requestAppId;
        if (string.IsNullOrWhiteSpace(appId))
        {
            throw new InvalidOperationException("微信小程序 AppId 未配置。");
        }

        return appId;
    }

    /// <summary>
    /// 获取请求使用的小程序 AppSecret。
    /// </summary>
    private string ResolveAppSecret()
    {
        if (string.IsNullOrWhiteSpace(_option.AppSecret))
        {
            throw new InvalidOperationException("微信小程序 AppSecret 未配置。");
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
