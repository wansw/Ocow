using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Ocow.InternalAuth.Extensions;
using Ocow.InternalAuth.Interfaces;
using Ocow.InternalAuth.Options;
using Ocow.InternalAuth.Services;

namespace Ocow.Tests.Unit;

/// <summary>
/// 内部服务认证测试，用于验证服务 Token 签发和 HttpClient 自动认证能力。
/// </summary>
public class InternalServiceAuthTests
{
    /// <summary>
    /// 验证服务 Token 签发器会生成可被内部服务校验的 JWT。
    /// </summary>
    [Fact]
    public void CreateToken_WhenCalled_ShouldSignInternalScopeToken()
    {
        var option = CreateInternalAuthOption();
        var tokenProvider = new InternalServiceTokenProvider(Options.Create(option));

        var token = tokenProvider.CreateToken();

        var principal = ValidateToken(token, option);
        Assert.Equal("internal", principal.FindFirst("scope")?.Value);
        Assert.Equal("Ocow.Jobs", principal.FindFirst("service_name")?.Value);
        Assert.Equal("Ocow.Jobs", principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value);
    }

    /// <summary>
    /// 验证内部服务认证处理器会自动附加 Bearer Token 和 HMAC 签名请求头。
    /// </summary>
    [Fact]
    public async Task SendAsync_WhenRequestSent_ShouldAttachBearerTokenAndHmacHeaders()
    {
        var option = CreateInternalAuthOption();
        var hmacService = new HmacSignatureService(Options.Create(new HmacSignatureOption
        {
            Secret = "UnitTestHmacSecret",
            TimestampToleranceSeconds = 300
        }));
        var tokenProvider = new InternalServiceTokenProvider(Options.Create(option));
        var captureHandler = new CaptureHttpMessageHandler();
        var authHandler = new InternalServiceAuthenticationHandler(tokenProvider, hmacService, Options.Create(option))
        {
            InnerHandler = captureHandler
        };
        using var client = new HttpClient(authHandler);
        using var content = new StringContent("{\"page\":1}", Encoding.UTF8, "application/json");

        await client.PostAsync("https://order.local/internal/orders/sync/erp", content);

        Assert.NotNull(captureHandler.Request);
        var request = captureHandler.Request;
        Assert.Equal("Bearer", request.Headers.Authorization?.Scheme);
        Assert.NotNull(request.Headers.Authorization?.Parameter);
        Assert.Equal("internal", ValidateToken(request.Headers.Authorization!.Parameter!, option).FindFirst("scope")?.Value);
        Assert.Equal("Ocow.Jobs", request.Headers.GetValues("X-Service-Name").Single());

        var timestamp = request.Headers.GetValues("X-Timestamp").Single();
        var nonce = request.Headers.GetValues("X-Nonce").Single();
        var bodyHash = request.Headers.GetValues("X-Body-SHA256").Single();
        var signature = request.Headers.GetValues("X-Signature").Single();
        var expectedBodyHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes("{\"page\":1}")));

        Assert.Equal(expectedBodyHash, bodyHash);
        Assert.True(hmacService.Validate("Ocow.Jobs", "POST", "/internal/orders/sync/erp", timestamp, nonce, bodyHash, signature));
    }

    /// <summary>
    /// 验证内部服务认证扩展会注册签发器和 HttpClient 认证处理器。
    /// </summary>
    [Fact]
    public async Task AddOcowInternalServiceAuthentication_WhenHttpClientSendsRequest_ShouldUseClientAuthHandler()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["InternalAuth:Issuer"] = "Ocow.InternalAuth.UnitTests",
                ["InternalAuth:Audience"] = "Ocow.InternalServices.UnitTests",
                ["InternalAuth:Secret"] = "UnitTestInternalServiceJwtSecret-123456",
                ["InternalAuth:ServiceName"] = "Ocow.Jobs",
                ["InternalAuth:TokenExpireMinutes"] = "5",
                ["HmacSignature:Secret"] = "UnitTestHmacSecret"
            })
            .Build();
        var captureHandler = new CaptureHttpMessageHandler();

        services.AddOcowInternalAuth(configuration);
        services.AddHttpClient("order", client => client.BaseAddress = new Uri("https://order.local"))
            .AddOcowInternalServiceAuthentication()
            .ConfigurePrimaryHttpMessageHandler(() => captureHandler);

        using var provider = services.BuildServiceProvider();
        Assert.NotNull(provider.GetRequiredService<IInternalServiceTokenProvider>());
        Assert.NotNull(provider.GetRequiredService<InternalServiceAuthenticationHandler>());

        var client = provider.GetRequiredService<IHttpClientFactory>().CreateClient("order");
        await client.GetAsync("/internal/orders/sync/erp");

        Assert.NotNull(captureHandler.Request);
        Assert.Equal("Bearer", captureHandler.Request.Headers.Authorization?.Scheme);
        Assert.Equal("Ocow.Jobs", captureHandler.Request.Headers.GetValues("X-Service-Name").Single());
    }

    private static InternalAuthOption CreateInternalAuthOption()
    {
        return new InternalAuthOption
        {
            Issuer = "Ocow.InternalAuth.UnitTests",
            Audience = "Ocow.InternalServices.UnitTests",
            Secret = "UnitTestInternalServiceJwtSecret-123456",
            ServiceName = "Ocow.Jobs",
            TokenExpireMinutes = 5
        };
    }

    private static ClaimsPrincipal ValidateToken(string token, InternalAuthOption option)
    {
        var handler = new JwtSecurityTokenHandler
        {
            MapInboundClaims = false
        };

        return handler.ValidateToken(token, new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = option.Issuer,
            ValidAudience = option.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(option.Secret)),
            ClockSkew = TimeSpan.Zero
        }, out _);
    }

    private sealed class CaptureHttpMessageHandler : HttpMessageHandler
    {
        public HttpRequestMessage? Request { get; private set; }

        /// <summary>
        /// 捕获测试请求并返回成功响应。
        /// </summary>
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Request = request;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }
    }
}
