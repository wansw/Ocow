using System.Net.Http.Headers;
using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using Ocow.InternalAuth.Interfaces;
using Ocow.InternalAuth.Options;

namespace Ocow.InternalAuth.Services;

/// <summary>
/// 内部服务 HttpClient 认证处理器，用于自动附加服务 JWT 和 HMAC 请求头。
/// </summary>
public class InternalServiceAuthenticationHandler : DelegatingHandler
{
    private readonly IInternalServiceTokenProvider _tokenProvider;
    private readonly IHmacSignatureService _hmacSignatureService;
    private readonly InternalAuthOption _option;

    /// <summary>
    /// 初始化内部服务 HttpClient 认证处理器。
    /// </summary>
    public InternalServiceAuthenticationHandler(
        IInternalServiceTokenProvider tokenProvider,
        IHmacSignatureService hmacSignatureService,
        IOptions<InternalAuthOption> option)
    {
        _tokenProvider = tokenProvider;
        _hmacSignatureService = hmacSignatureService;
        _option = option.Value;
    }

    /// <summary>
    /// 发送内部服务请求前附加服务认证信息。
    /// </summary>
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _tokenProvider.CreateToken());
        await AddHmacHeadersAsync(request, cancellationToken);

        return await base.SendAsync(request, cancellationToken);
    }

    /// <summary>
    /// 为当前请求附加 HMAC 签名相关请求头。
    /// </summary>
    private async Task AddHmacHeadersAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var timestamp = DateTimeOffset.UtcNow.ToString("O");
        var nonce = Guid.NewGuid().ToString("N");
        var bodyHash = await ComputeBodyHashAsync(request.Content, cancellationToken);
        var path = request.RequestUri?.AbsolutePath ?? "/";
        var signature = _hmacSignatureService.Generate(_option.ServiceName, request.Method.Method, path, timestamp, nonce, bodyHash);

        SetHeader(request.Headers, "X-Service-Name", _option.ServiceName);
        SetHeader(request.Headers, "X-Timestamp", timestamp);
        SetHeader(request.Headers, "X-Nonce", nonce);
        SetHeader(request.Headers, "X-Body-SHA256", bodyHash);
        SetHeader(request.Headers, "X-Signature", signature);
    }

    /// <summary>
    /// 计算请求体 SHA256 摘要，空请求体返回空字符串。
    /// </summary>
    private static async Task<string> ComputeBodyHashAsync(HttpContent? content, CancellationToken cancellationToken)
    {
        if (content is null)
        {
            return string.Empty;
        }

        var body = await content.ReadAsByteArrayAsync(cancellationToken);
        return Convert.ToHexString(SHA256.HashData(body));
    }

    /// <summary>
    /// 设置请求头，避免重复添加同名请求头。
    /// </summary>
    private static void SetHeader(HttpRequestHeaders headers, string name, string value)
    {
        headers.Remove(name);
        headers.TryAddWithoutValidation(name, value);
    }
}
