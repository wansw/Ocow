using OrderEntity = Ocow.Order.Domain.Models.Order;

namespace Ocow.Order.Application.Interfaces;

/// <summary>
/// 订单仓储接口，用于隔离应用层和数据库实现。
/// </summary>
public interface IOrderRepository
{
    /// <summary>
    /// 新增订单。
    /// </summary>
    Task AddAsync(OrderEntity order, CancellationToken cancellationToken = default);

    /// <summary>
    /// 按订单编号查询订单。
    /// </summary>
    Task<OrderEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 按外部来源和外部订单编号查询订单，用于 ERP 同步幂等判断。
    /// </summary>
    Task<OrderEntity?> GetByExternalOrderAsync(string sourceSystem, string externalOrderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 分页查询指定会员的订单。
    /// </summary>
    Task<(IReadOnlyList<OrderEntity> Items, long Total)> GetCustomerOrdersAsync(Guid customerId, int pageIndex, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// 分页查询后台订单列表。
    /// </summary>
    Task<(IReadOnlyList<OrderEntity> Items, long Total)> GetAdminOrdersAsync(int pageIndex, int pageSize, CancellationToken cancellationToken = default);
}
