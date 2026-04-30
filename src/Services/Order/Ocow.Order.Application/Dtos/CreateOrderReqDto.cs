namespace Ocow.Order.Application.Dtos;

/// <summary>
/// 创建订单请求 DTO。
/// </summary>
public class CreateOrderReqDto
{
    public Guid CustomerId { get; init; }

    public string RequestId { get; init; } = string.Empty;

    public List<OrderItemReqDto> Items { get; init; } = new();
}
