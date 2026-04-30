using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ocow.MessageBus.Interfaces;
using Ocow.MessageBus.Options;

namespace Ocow.MessageBus.Extensions;

/// <summary>
/// RabbitMQ 消息总线注册扩展。
/// </summary>
public static class MessageBusServiceCollectionExtensions
{
    /// <summary>
    /// 注册消息总线配置和发布服务占位实现。
    /// </summary>
    public static IServiceCollection AddOcowMessageBus(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RabbitMqOption>(configuration.GetSection("RabbitMq"));
        services.AddSingleton<IMessagePublisher, NoopMessagePublisher>();
        return services;
    }

    private class NoopMessagePublisher : IMessagePublisher
    {
        /// <summary>
        /// MVP 阶段的空发布实现，用于先固定应用层依赖边界。
        /// </summary>
        public Task PublishAsync<T>(string routingKey, T message, string traceId, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
