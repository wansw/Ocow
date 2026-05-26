using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Ocow.InternalAuth.Interfaces;
using Ocow.InternalAuth.Options;
using Ocow.InternalAuth.Services;

namespace Ocow.InternalAuth.Extensions;

/// <summary>
/// 内部服务认证注册扩展，用于接入 Service JWT 和 HMAC 签名能力。
/// </summary>
public static class InternalAuthServiceCollectionExtensions
{
    public const string ServiceJwtScheme = "ServiceJwt";

    public const string InternalOnlyPolicy = "InternalOnly";

    /// <summary>
    /// 注册内部服务 JWT 校验策略和 HMAC 签名服务。
    /// </summary>
    public static IServiceCollection AddOcowInternalAuth(this IServiceCollection services, IConfiguration configuration)
    {
        var option = configuration.GetSection("InternalAuth").Get<InternalAuthOption>() ?? new InternalAuthOption();

        services.Configure<InternalAuthOption>(configuration.GetSection("InternalAuth"));
        services.Configure<HmacSignatureOption>(configuration.GetSection("HmacSignature"));
        services.AddSingleton<IHmacSignatureService, HmacSignatureService>();
        services.AddSingleton<IInternalServiceTokenProvider, InternalServiceTokenProvider>();
        services.AddTransient<InternalServiceAuthenticationHandler>();

        services.AddAuthentication()
            .AddJwtBearer(ServiceJwtScheme, options => ConfigureJwt(options, option.Issuer, option.Audience, option.Secret));

        services.AddAuthorization(options =>
        {
            options.AddPolicy(InternalOnlyPolicy, policy => RequireScope(policy, ServiceJwtScheme, "internal"));
        });

        return services;
    }

    /// <summary>
    /// 为 HttpClient 注册内部服务自动认证处理器。
    /// </summary>
    public static IHttpClientBuilder AddOcowInternalServiceAuthentication(this IHttpClientBuilder builder)
    {
        return builder.AddHttpMessageHandler<InternalServiceAuthenticationHandler>();
    }

    /// <summary>
    /// 配置 JWT Bearer Token 校验参数。
    /// </summary>
    private static void ConfigureJwt(JwtBearerOptions options, string issuer, string audience, string secret)
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret))
        };
    }

    /// <summary>
    /// 配置指定认证方案的 scope 要求。
    /// </summary>
    private static void RequireScope(AuthorizationPolicyBuilder policy, string scheme, string scope)
    {
        policy.AuthenticationSchemes.Add(scheme);
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("scope", scope);
    }
}
