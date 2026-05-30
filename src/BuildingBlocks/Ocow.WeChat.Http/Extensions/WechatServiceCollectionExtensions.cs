using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Ocow.WeChat.Abstractions.Interfaces;
using Ocow.WeChat.Core.Options;
using Ocow.WeChat.Http.Services;

namespace Ocow.WeChat.Http.Extensions;

/// <summary>
/// 微信开放接口服务注册扩展，用于注册微信配置和 HTTP 客户端。
/// </summary>
public static class WechatServiceCollectionExtensions
{
    /// <summary>
    /// 注册微信开放接口客户端。
    /// </summary>
    public static IServiceCollection AddOcowWechat(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<WechatMiniProgramOption>(configuration.GetSection("Wechat:MiniProgram"));
        services.Configure<WechatOfficialAccountOption>(configuration.GetSection("Wechat:OfficialAccount"));

        services.AddHttpClient<IWechatMiniProgramClient, WechatMiniProgramClient>((provider, client) =>
        {
            var option = provider.GetRequiredService<IOptions<WechatMiniProgramOption>>().Value;
            client.BaseAddress = new Uri(NormalizeBaseUrl(option.ApiBaseUrl));
            client.Timeout = TimeSpan.FromSeconds(10);
        });

        services.AddHttpClient<IWechatOfficialAccountClient, WechatOfficialAccountClient>((provider, client) =>
        {
            var option = provider.GetRequiredService<IOptions<WechatOfficialAccountOption>>().Value;
            client.BaseAddress = new Uri(NormalizeBaseUrl(option.ApiBaseUrl));
            client.Timeout = TimeSpan.FromSeconds(10);
        });

        return services;
    }

    /// <summary>
    /// 规范化微信 API 基础地址。
    /// </summary>
    private static string NormalizeBaseUrl(string? apiBaseUrl)
    {
        var value = string.IsNullOrWhiteSpace(apiBaseUrl)
            ? "https://api.weixin.qq.com"
            : apiBaseUrl.Trim();

        return value.EndsWith("/", StringComparison.Ordinal) ? value : $"{value}/";
    }
}
