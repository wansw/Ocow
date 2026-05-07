using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Ocow.EntityFrameworkCore.Abstractions;
using Ocow.EntityFrameworkCore.Interceptors;
using Ocow.EntityFrameworkCore.UnitOfWork;

namespace Ocow.EntityFrameworkCore.Extensions;

/// <summary>
/// EF Core 服务注册扩展。
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 注册 Ocow EF Core 通用拦截器。
    /// </summary>
    public static IServiceCollection AddOcowEntityFrameworkCore(this IServiceCollection services)
    {
        services.AddSingleton<AuditableEntitySaveChangesInterceptor>();
        services.AddSingleton<SoftDeleteSaveChangesInterceptor>();
        services.AddSingleton<SaveChangesInterceptor, AuditableEntitySaveChangesInterceptor>();
        services.AddSingleton<SaveChangesInterceptor, SoftDeleteSaveChangesInterceptor>();

        return services;
    }

    /// <summary>
    /// 注册指定 DbContext 对应的 EF Core 工作单元。
    /// </summary>
    public static IServiceCollection AddOcowUnitOfWork<TDbContext>(this IServiceCollection services)
        where TDbContext : DbContext
    {
        services.AddScoped<IUnitOfWork, EfUnitOfWork<TDbContext>>();
        return services;
    }
}
