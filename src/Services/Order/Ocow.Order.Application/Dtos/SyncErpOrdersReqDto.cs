namespace Ocow.Order.Application.Dtos;

/// <summary>
/// ERP 订单同步请求 DTO。
/// </summary>
public class SyncErpOrdersReqDto
{
    public DateTime? BeginTime { get; init; }

    public DateTime? EndTime { get; init; }
}
