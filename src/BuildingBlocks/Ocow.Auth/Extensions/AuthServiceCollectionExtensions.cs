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

    public const string CustomerOnlyPolicy = "CustomerOnly";
    public const string AdminOnlyPolicy = "AdminOnly";

    /// <summary>
    /// 注册用户侧 JWT 校验、权限点授权和 Admin Redis 会话校验。
    /// </summary>
    public static IServiceCollection AddOcowAuth(this IServiceCollection services, IConfiguration configuration)
    {
        var identityOption = configuration.GetSection("Jwt").Get<IdentityJwtOption>() ?? new IdentityJwtOption();

        services.Configure<IdentityJwtOption>(configuration.GetSection("Jwt"));
        services.AddScoped<IAdminTokenSessionValidator, RedisAdminTokenSessionValidator>();
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = CustomerJwtScheme;
                options.DefaultChallengeScheme = CustomerJwtScheme;
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

        services.AddAuthorization(options =>
        {
            options.AddPolicy(CustomerOnlyPolicy, policy => RequireScope(policy, CustomerJwtScheme, "client"));
            options.AddPolicy(AdminOnlyPolicy, policy => RequireScope(policy, AdminJwtScheme, "admin"));
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
        policy.AuthenticationSchemes.Add(scheme);
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("scope", scope);
    }
}
