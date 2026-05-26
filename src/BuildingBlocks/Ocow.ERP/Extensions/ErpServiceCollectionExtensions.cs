using Microsoft.Extensions.DependencyInjection;
using Ocow.ERP.Interfaces;
using Ocow.ERP.Services;

namespace Ocow.ERP.Extensions;

/// <summary>
/// ERP 服务注册扩展，用于接入外部 ERP 适配能力。
/// </summary>
public static class ErpServiceCollectionExtensions
{
    /// <summary>
    /// 注册 ERP 订单客户端工厂和默认 Demo ERP 客户端。
    /// </summary>
    public static IServiceCollection AddOcowErp(this IServiceCollection services)
    {
        services.AddSingleton<DemoErpOrderClient>();
        services.AddSingleton<IErpClientFactory, DefaultErpClientFactory>();

        return services;
    }
}
