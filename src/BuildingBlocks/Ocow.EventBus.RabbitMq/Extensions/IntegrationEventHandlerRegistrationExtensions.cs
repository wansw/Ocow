using Microsoft.Extensions.DependencyInjection;
using Ocow.Contracts.Abstractions;
using Ocow.EventBus.Abstractions.Interfaces;

namespace Ocow.EventBus.RabbitMq.Extensions;

/// <summary>
/// 集成事件处理器注册扩展。
/// </summary>
public static class IntegrationEventHandlerRegistrationExtensions
{
    /// <summary>
    /// 注册指定集成事件的业务处理器。
    /// </summary>
    public static IServiceCollection AddIntegrationEventHandler<TEvent, THandler>(this IServiceCollection services)
        where TEvent : IntegrationEvent
        where THandler : class, IIntegrationEventHandler<TEvent>
    {
        services.AddScoped<IIntegrationEventHandler<TEvent>, THandler>();
        return services;
    }
}
