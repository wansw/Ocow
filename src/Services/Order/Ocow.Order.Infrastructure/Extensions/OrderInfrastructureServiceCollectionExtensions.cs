using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ocow.EntityFrameworkCore.Extensions;
using Ocow.EntityFrameworkCore.Options;
using Ocow.Order.Application.Interfaces;
using Ocow.Order.Infrastructure.Data;
using Ocow.Order.Infrastructure.Repositories;
using Ocow.Order.Infrastructure.Services;

namespace Ocow.Order.Infrastructure.Extensions;

/// <summary>
/// 订单基础设施层服务注册扩展
/// </summary>
public static class OrderInfrastructureServiceCollectionExtensions
{
    /// <summary>
    /// 注册订单数据库上下文和仓储实现。    
    /// </summary>
    public static IServiceCollection AddOrderInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        //var option = configuration.GetSection("Database").Get<DatabaseOption>() ?? new DatabaseOption();
        //services.Configure<DatabaseOption>(configuration.GetSection("Database"));
        //services.AddOcowEntityFrameworkCore();
        //services.AddDbContext<OrderDbContext>(builder => builder
        //    .UseOcowDatabase(option)
        //    .AddOcowSaveChangesInterceptors());
        //services.AddOcowUnitOfWork<OrderDbContext>();
        //services.AddScoped<IOrderRepository, OrderRepository>();

        services.AddOcowDbContext<OrderDbContext>(configuration, repos =>
        {
            repos.AddScoped<IOrderRepository, OrderRepository>();
            repos.AddScoped<IOrderCreationTransaction, CapOrderCreationTransaction>();
        });

        return services;
    }
}
