namespace Ocow.Order.Application.Dtos;

/// <summary>
/// ERP 订单同步响应 DTO，用于返回本次同步统计结果。
/// </summary>
public class SyncErpOrdersResDto
{
    /// <summary>
    /// 本次同步写入订单数量。
    /// </summary>
    public int SyncedCount { get; init; }

    /// <summary>
    /// 本次同步跳过重复订单数量。
    /// </summary>
    public int SkippedCount { get; init; }

    /// <summary>
    /// 实际同步开始时间。
    /// </summary>
    public DateTimeOffset FromTime { get; init; }

    /// <summary>
    /// 实际同步结束时间。
    /// </summary>
    public DateTimeOffset ToTime { get; init; }
}
