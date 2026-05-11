using System.Security.Claims;
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
/// 内部服务认证注册扩展。
/// </summary>
public static class InternalAuthServiceCollectionExtensions
{
    public const string CustomerJwtScheme = "CustomerJwt";
    public const string AdminJwtScheme = "AdminJwt";
    public const string ServiceJwtScheme = "ServiceJwt";

    public const string CustomerOnlyPolicy = "CustomerOnly";
    public const string AdminOnlyPolicy = "AdminOnly";
    public const string InternalOnlyPolicy = "InternalOnly";
    public const string OrderShipPolicy = "order.ship";

    /// <summary>
    /// 注册 Service JWT 验证策略。
    /// </summary>
    public static IServiceCollection AddOcowInternalAuth(this IServiceCollection services, IConfiguration configuration)
    {
        var option = configuration.GetSection("InternalAuth").Get<InternalAuthOption>() ?? new InternalAuthOption();
        services.Configure<InternalAuthOption>(configuration.GetSection("InternalAuth"));

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options => ConfigureJwt(options, option.Issuer, option.Audience, option.Secret));

        return services;
    }

    /// <summary>
    /// 注册 Customer、Admin、Service 三类 JWT 校验策略和权限策略。
    /// </summary>
    public static IServiceCollection AddOcowJwtAuthorization(this IServiceCollection services, IConfiguration configuration)
    {
        var identityOption = configuration.GetSection("Jwt").Get<IdentityJwtOption>() ?? new IdentityJwtOption();
        var internalOption = configuration.GetSection("InternalAuth").Get<InternalAuthOption>() ?? new InternalAuthOption();

        services.Configure<IdentityJwtOption>(configuration.GetSection("Jwt"));
        services.Configure<InternalAuthOption>(configuration.GetSection("InternalAuth"));
        services.Configure<HmacSignatureOption>(configuration.GetSection("HmacSignature"));

        services.AddSingleton<IHmacSignatureService, HmacSignatureService>();
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
            })
            .AddJwtBearer(ServiceJwtScheme, options => ConfigureJwt(options, internalOption.Issuer, internalOption.Audience, internalOption.Secret));

        services.AddAuthorization(options =>
        {
            options.AddPolicy(CustomerOnlyPolicy, policy => RequireScope(policy, CustomerJwtScheme, "client"));
            options.AddPolicy(AdminOnlyPolicy, policy => RequireScope(policy, AdminJwtScheme, "admin"));
            options.AddPolicy(InternalOnlyPolicy, policy => RequireScope(policy, ServiceJwtScheme, "internal"));
            options.AddPolicy(OrderShipPolicy, policy =>
            {
                RequireScope(policy, AdminJwtScheme, "admin");
                policy.RequireClaim("permission", "order.ship");
            });
        });

        return services;
    }

    /// <summary>
    /// 配置 JWT Bearer 校验参数。
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
    /// 在 Admin JWT 认证成功后继续校验 Redis 集中式会话状态。
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
