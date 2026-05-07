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
        // 从配置中获取数据库选项，如果没有配置，则使用默认值。
        var option = configuration.GetSection("Database").Get<DatabaseOption>() ?? new DatabaseOption();
        // 将数据库选项绑定到配置系统，以便在应用程序中使用 IOptions<DatabaseOption> 注入。
        services.Configure<DatabaseOption>(configuration.GetSection("Database"));

        //注册 Ocow EF Core 通用拦截器。 
        services.AddOcowEntityFrameworkCore();

        services.AddDbContext<IdentityDbContext>(builder => builder
                .UseOcowDatabase(option)
                .AddOcowSaveChangesInterceptors());

        services.AddOcowUnitOfWork<IdentityDbContext>();
        services.AddScoped<IIdentityRepository, IdentityRepository>();

        return services;
    }
}
