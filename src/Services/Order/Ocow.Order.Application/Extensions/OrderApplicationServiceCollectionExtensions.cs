using Microsoft.Extensions.DependencyInjection;
using Ocow.Order.Application.Interfaces;
using Ocow.Order.Application.Services;

namespace Ocow.Order.Application.Extensions;

/// <summary>
/// 订单应用层服务注册扩展。
/// </summary>
public static class OrderApplicationServiceCollectionExtensions
{
    /// <summary>
    /// 注册订单应用服务。
    /// </summary>
    public static IServiceCollection AddOrderApplication(this IServiceCollection services)
    {
        services.AddScoped<IOrderAppService, OrderAppService>();
        return services;
    }
}
