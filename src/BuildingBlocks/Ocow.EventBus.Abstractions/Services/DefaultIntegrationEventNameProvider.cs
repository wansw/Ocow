using System.Reflection;
using System.Text;
using Ocow.Contracts.Abstractions;
using Ocow.EventBus.Abstractions.Interfaces;

namespace Ocow.EventBus.Abstractions.Services;

/// <summary>
/// 默认集成事件名称提供器，优先读取显式特性，未标注时按类型名推导。
/// </summary>
public sealed class DefaultIntegrationEventNameProvider : IIntegrationEventNameProvider
{
    /// <summary>
    /// 获取指定集成事件类型对应的事件名称。
    /// </summary>
    public string GetName<TEvent>()
        where TEvent : IntegrationEvent
    {
        return GetName(typeof(TEvent));
    }

    /// <summary>
    /// 获取指定运行时类型对应的事件名称。
    /// </summary>
    public string GetName(Type eventType)
    {
        ArgumentNullException.ThrowIfNull(eventType);

        if (!typeof(IntegrationEvent).IsAssignableFrom(eventType))
        {
            throw new ArgumentException($"类型 {eventType.FullName} 必须继承 {nameof(IntegrationEvent)}。", nameof(eventType));
        }

        var attribute = eventType.GetCustomAttribute<IntegrationEventNameAttribute>();
        if (attribute is not null)
        {
            return attribute.Name;
        }

        var name = eventType.Name;
        if (name.EndsWith("IntegrationEvent", StringComparison.Ordinal))
        {
            name = name[..^"IntegrationEvent".Length];
        }

        return $"ocow.{ToDotCase(name)}";
    }

    /// <summary>
    /// 将 PascalCase 类型名转换为 dot-case 事件名片段。
    /// </summary>
    private static string ToDotCase(string value)
    {
        var builder = new StringBuilder();

        for (var i = 0; i < value.Length; i++)
        {
            var current = value[i];
            if (char.IsUpper(current))
            {
                if (i > 0)
                {
                    builder.Append('.');
                }

                builder.Append(char.ToLowerInvariant(current));
                continue;
            }

            builder.Append(current);
        }

        return builder.ToString();
    }
}
