using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Ocow.Auth.Interfaces;
using Ocow.Auth.Options;
using Ocow.Auth.Services;

namespace Ocow.Auth.Extensions;

/// <summary>
/// 用户侧认证授权注册扩展，用于接入 Admin/Customer JWT、权限点授权和 Admin Redis 会话校验。
/// </summary>
public static class AuthServiceCollectionExtensions
{
    public const string CustomerJwtScheme = "CustomerJwt";
    public const string AdminJwtScheme = "AdminJwt";
    public const string GatewayUserJwtScheme = "GatewayUserJwt";

    public const string CustomerOnlyPolicy = "CustomerOnly";
    public const string AdminOnlyPolicy = "AdminOnly";

    /// <summary>
    /// 注册用户侧 JWT 校验、权限点授权和 Admin Redis 会话校验。
    /// </summary>
    public static IServiceCollection AddOcowAuth(this IServiceCollection services, IConfiguration configuration)
    {
        var identityOption = configuration.GetSection("Jwt").Get<IdentityJwtOption>() ?? new IdentityJwtOption();
        var gatewayOption = configuration.GetSection("GatewayForwardedJwt").Get<GatewayForwardedJwtOption>() ?? new GatewayForwardedJwtOption();

        services.Configure<IdentityJwtOption>(configuration.GetSection("Jwt"));
        services.Configure<GatewayForwardedJwtOption>(configuration.GetSection("GatewayForwardedJwt"));
        services.AddScoped<IAdminTokenSessionValidator, RedisAdminTokenSessionValidator>();
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

        var authenticationBuilder = services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = gatewayOption.Enabled ? GatewayUserJwtScheme : CustomerJwtScheme;
                options.DefaultChallengeScheme = gatewayOption.Enabled ? GatewayUserJwtScheme : CustomerJwtScheme;
            })
            .AddJwtBearer(CustomerJwtScheme, options => ConfigureJwt(options, identityOption.Issuer, identityOption.Audience, identityOption.Secret))
            .AddJwtBearer(AdminJwtScheme, options =>
            {
                ConfigureJwt(options, identityOption.Issuer, identityOption.Audience, identityOption.Secret);
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = ValidateAdminTokenSessionAsync
                };
            });

        if (gatewayOption.Enabled)
        {
            authenticationBuilder.AddJwtBearer(GatewayUserJwtScheme, options => ConfigureJwt(options, gatewayOption.Issuer, gatewayOption.Audience, gatewayOption.Secret));
        }

        services.AddAuthorization(options =>
        {
            options.AddPolicy(CustomerOnlyPolicy, policy => RequireScope(policy, GetCustomerSchemes(gatewayOption), "client"));
            options.AddPolicy(AdminOnlyPolicy, policy => RequireScope(policy, GetAdminSchemes(gatewayOption), "admin"));
        });

        return services;
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
    /// 在 Admin JWT 校验通过后继续校验 Redis 集中式会话状态。
    /// </summary>
    private static async Task ValidateAdminTokenSessionAsync(TokenValidatedContext context)
    {
        try
        {
            var validator = context.HttpContext.RequestServices.GetRequiredService<IAdminTokenSessionValidator>();
            var isValid = await validator.ValidateAsync(context.Principal ?? new ClaimsPrincipal());
            if (!isValid)
            {
                context.Fail("Admin Token 会话已失效。");
            }
        }
        catch
        {
            context.Fail("Admin Token 会话校验失败。");
        }
    }

    /// <summary>
    /// 配置指定认证方案的 scope 要求。
    /// </summary>
    private static void RequireScope(AuthorizationPolicyBuilder policy, string scheme, string scope)
    {
        RequireScope(policy, [scheme], scope);
    }

    /// <summary>
    /// 配置指定认证方案集合的 scope 要求。
    /// </summary>
    private static void RequireScope(AuthorizationPolicyBuilder policy, IEnumerable<string> schemes, string scope)
    {
        foreach (var scheme in schemes)
        {
            policy.AuthenticationSchemes.Add(scheme);
        }

        policy.RequireAuthenticatedUser();
        policy.RequireClaim("scope", scope);
    }

    /// <summary>
    /// 获取客户接口可接受的认证方案集合。
    /// </summary>
    internal static IReadOnlyList<string> GetCustomerSchemes(GatewayForwardedJwtOption option)
    {
        if (!option.Enabled)
        {
            return [CustomerJwtScheme];
        }

        return option.AllowDirectIdentityJwt
            ? [GatewayUserJwtScheme, CustomerJwtScheme]
            : [GatewayUserJwtScheme];
    }

    /// <summary>
    /// 获取后台接口可接受的认证方案集合。
    /// </summary>
    internal static IReadOnlyList<string> GetAdminSchemes(GatewayForwardedJwtOption option)
    {
        if (!option.Enabled)
        {
            return [AdminJwtScheme];
        }

        return option.AllowDirectIdentityJwt
            ? [GatewayUserJwtScheme, AdminJwtScheme]
            : [GatewayUserJwtScheme];
    }
}
