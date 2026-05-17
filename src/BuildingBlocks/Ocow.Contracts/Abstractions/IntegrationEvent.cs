namespace Ocow.Contracts.Abstractions;

/// <summary>
/// 集成事件基类，用于定义跨服务事件的公共追踪和幂等字段。
/// </summary>
public abstract record IntegrationEvent
{
    /// <summary>
    /// 事件唯一编号，用于幂等处理、链路追踪和问题排查。
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// 事件发生的 UTC 时间。
    /// </summary>
    public DateTime OccurredOnUtc { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// 业务链路关联编号，用于串联同一业务流程中的多个事件。
    /// </summary>
    public string? CorrelationId { get; init; }

    /// <summary>
    /// 触发当前事件的上游命令或事件编号。
    /// </summary>
    public string? CausationId { get; init; }
}
