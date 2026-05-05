using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ocow.EntityFrameworkCore.Extensions;
using Ocow.EntityFrameworkCore.Options;
using Ocow.Identity.Application.Interfaces;
using Ocow.Identity.Infrastructure.Data;
using Ocow.Identity.Infrastructure.Repositories;

namespace Ocow.Identity.Infrastructure.Extensions;

/// <summary>
/// 身份认证基础设施服务注册扩展。/// </summary>
public static class IdentityInfrastructureServiceCollectionExtensions
{
    /// <summary>
    /// 注册身份服务数据库上下文和仓储实现。    /// </summary>
    public static IServiceCollection AddIdentityInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var option = configuration.GetSection("Database").Get<DatabaseOption>() ?? new DatabaseOption();
        services.Configure<DatabaseOption>(configuration.GetSection("Database"));
        services.AddOcowEntityFrameworkCore();
        services.AddDbContext<IdentityDbContext>(builder => builder
                .UseOcowDatabase(option)
                .AddOcowSaveChangesInterceptors());
        services.AddScoped<IIdentityRepository, IdentityRepository>();

        return services;
    }
}
