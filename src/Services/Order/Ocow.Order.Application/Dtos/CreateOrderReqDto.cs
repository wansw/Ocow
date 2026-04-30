namespace Ocow.Order.Application.Dtos;

/// <summary>
/// 创建订单请求 DTO。
/// </summary>
public class CreateOrderReqDto
{
    /// <summary>
    /// 下单会员编号。
    /// </summary>
    public Guid CustomerId { get; init; }

    /// <summary>
    /// 客户端请求编号，用于后续下单幂等控制。
    /// </summary>
    public string RequestId { get; init; } = string.Empty;

    /// <summary>
    /// 订单商品明细列表。
    /// </summary>
    public List<OrderItemReqDto> Items { get; init; } = new();
}
