namespace Ocow.Contracts.Abstractions;

/// <summary>
/// 集成事件名称特性，用于显式声明跨服务事件的稳定发布名称。
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class IntegrationEventNameAttribute : Attribute
{
    /// <summary>
    /// 创建集成事件名称特性。
    /// </summary>
    public IntegrationEventNameAttribute(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("集成事件名称不能为空。", nameof(name));
        }

        Name = name;
    }

    /// <summary>
    /// 跨服务事件名称，上线后不得随意修改。
    /// </summary>
    public string Name { get; }
}
