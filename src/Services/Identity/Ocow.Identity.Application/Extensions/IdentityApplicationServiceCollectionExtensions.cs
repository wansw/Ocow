using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ocow.Identity.Application.Interfaces;
using Ocow.Identity.Application.Options;
using Ocow.Identity.Application.Services;

namespace Ocow.Identity.Application.Extensions;

/// <summary>
/// 身份认证应用层服务注册扩展。
/// </summary>
public static class IdentityApplicationServiceCollectionExtensions
{
    /// <summary>
    /// 注册身份认证应用服务和 Token 服务。
    /// </summary>
    public static IServiceCollection AddIdentityApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtTokenOption>(configuration.GetSection("Jwt"));
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IAdminAuthAppService, AdminAuthAppService>();
        services.AddScoped<IClientAuthAppService, ClientAuthAppService>();
        services.AddScoped<IAdminUserAppService, AdminUserAppService>();
        services.AddScoped<IRolePermissionAppService, RolePermissionAppService>();

        return services;
    }
}
