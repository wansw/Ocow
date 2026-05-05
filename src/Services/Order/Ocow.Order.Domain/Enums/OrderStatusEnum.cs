namespace Ocow.Order.Domain.Enums;

/// <summary>
/// 订单状态枚举，用于表达订单生命周期。/// </summary>
public enum OrderStatusEnum
{
    PendingPay = 1,
    Paid = 2,
    Shipped = 3,
    Completed = 4,
    Canceled = 5
}
