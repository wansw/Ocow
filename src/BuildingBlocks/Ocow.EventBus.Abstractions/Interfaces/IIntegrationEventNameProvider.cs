using Ocow.Contracts.Abstractions;

namespace Ocow.EventBus.Abstractions.Interfaces;

/// <summary>
/// 集成事件名称提供器，用于根据事件类型获取稳定的事件名称。
/// </summary>
public interface IIntegrationEventNameProvider
{
    /// <summary>
    /// 获取指定集成事件类型对应的事件名称。
    /// </summary>
    string GetName<TEvent>()
        where TEvent : IntegrationEvent;

    /// <summary>
    /// 获取指定运行时类型对应的事件名称。
    /// </summary>
    string GetName(Type eventType);
}
