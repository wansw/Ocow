using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ocow.EntityFrameworkCore.Extensions;
using Ocow.Inventory.Infrastructure.Data;

namespace Ocow.Inventory.Infrastructure.Extensions;

/// <summary>
/// 库存基础设施层服务注册扩展。
/// </summary>
public static class InventoryInfrastructureServiceCollectionExtensions
{
    /// <summary>
    /// 注册库存数据库上下文。
    /// </summary>
    public static IServiceCollection AddInventoryInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOcowDbContext<InventoryDbContext>(configuration);
        return services;
    }
}
