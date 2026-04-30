using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ocow.Order.Application.Interfaces;
using Ocow.Order.Infrastructure.Data;
using Ocow.Order.Infrastructure.Options;
using Ocow.Order.Infrastructure.Repositories;

namespace Ocow.Order.Infrastructure.Extensions;

/// <summary>
/// 订单基础设施层服务注册扩展。
/// </summary>
public static class OrderInfrastructureServiceCollectionExtensions
{
    /// <summary>
    /// 注册订单数据库上下文和仓储实现。
    /// </summary>
    public static IServiceCollection AddOrderInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var option = configuration.GetSection("PostgreSql").Get<PostgreSqlOption>() ?? new PostgreSqlOption();
        services.Configure<PostgreSqlOption>(configuration.GetSection("PostgreSql"));
        services.AddDbContext<OrderDbContext>(builder => builder.UseNpgsql(option.ConnectionString));
        services.AddScoped<IOrderRepository, OrderRepository>();

        return services;
    }
}
