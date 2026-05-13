namespace Ocow.Observability.Options;

/// <summary>
/// 可观测性配置项，用于配置结构化日志和链路追踪的基础属性。
/// </summary>
public class ObservabilityOption
{
    /// <summary>
    /// 当前服务名称，用于写入每条结构化日志。
    /// </summary>
    public string ServiceName { get; set; } = string.Empty;
}
