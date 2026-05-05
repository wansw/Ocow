using Ocow.Order.Domain.Enums;
using Ocow.Order.Domain.Models;
using OrderEntity = Ocow.Order.Domain.Models.Order;

namespace Ocow.Tests.Unit;

/// <summary>
/// 订单领域实体测试，用于验证核心状态和金额规则。/// </summary>
public class OrderEntityTests
{
    /// <summary>
    /// 验证创建订单时会正确计算总金额。    /// </summary>
    [Fact]
    public void Create_ShouldCalculateTotalAmount()
    {
        var items = new[]
        {
            new OrderItem { ProductId = Guid.NewGuid(), SkuId = Guid.NewGuid(), ProductName = "娴嬭瘯鍟嗗搧A", Quantity = 2, UnitPrice = 10 },
            new OrderItem { ProductId = Guid.NewGuid(), SkuId = Guid.NewGuid(), ProductName = "娴嬭瘯鍟嗗搧B", Quantity = 1, UnitPrice = 5 }
        };

        var order = OrderEntity.Create(Guid.NewGuid(), items);

        Assert.Equal(25, order.TotalAmount);
        Assert.Equal(OrderStatusEnum.PendingPay, order.Status);
    }

    /// <summary>
    /// 验证待支付订单可以被取消。    /// </summary>
    [Fact]
    public void Cancel_WhenPendingPay_ShouldChangeStatus()
    {
        var order = OrderEntity.Create(Guid.NewGuid(), new[]
        {
            new OrderItem { ProductId = Guid.NewGuid(), SkuId = Guid.NewGuid(), ProductName = "娴嬭瘯鍟嗗搧", Quantity = 1, UnitPrice = 10 }
        });

        order.Cancel();

        Assert.Equal(OrderStatusEnum.Canceled, order.Status);
        Assert.NotNull(order.CanceledAt);
    }
}
