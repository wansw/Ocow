using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ocow.Identity.Application.Interfaces;
using Ocow.Identity.Application.Options;
using Ocow.Identity.Application.Services;

namespace Ocow.Identity.Application.Extensions;

/// <summary>
/// 注册应用服务
/// </summary>
public static class IdentityApplicationServiceCollectionExtensions
{
    /// <summary>
    /// 注册应用服务
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static IServiceCollection AddIdentityApplication(this IServiceCollection services, IConfiguration configuration)
    {
        // 从配置中获取 JWT 选项，如果没有配置，则使用默认值。
        services.Configure<JwtTokenOption>(configuration.GetSection("Jwt"));
        // 注册应用服务
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IAdminAuthAppService, AdminAuthAppService>();
        services.AddScoped<IClientAuthAppService, ClientAuthAppService>();
        services.AddScoped<IAdminUserAppService, AdminUserAppService>();
        services.AddScoped<IRolePermissionAppService, RolePermissionAppService>();

        return services;
    }
}
