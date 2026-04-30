using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Ocow.InternalAuth.Options;

namespace Ocow.InternalAuth.Extensions;

/// <summary>
/// 内部服务认证注册扩展。
/// </summary>
public static class InternalAuthServiceCollectionExtensions
{
    /// <summary>
    /// 注册 Service JWT 验证策略。
    /// </summary>
    public static IServiceCollection AddOcowInternalAuth(this IServiceCollection services, IConfiguration configuration)
    {
        var option = configuration.GetSection("InternalAuth").Get<InternalAuthOption>() ?? new InternalAuthOption();
        services.Configure<InternalAuthOption>(configuration.GetSection("InternalAuth"));

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = option.Issuer,
                    ValidAudience = option.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(option.Secret))
                };
            });

        return services;
    }
}
