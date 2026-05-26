using Microsoft.EntityFrameworkCore;
using Ocow.EntityFrameworkCore.Repositories;
using Ocow.Order.Application.Interfaces;
using Ocow.Order.Infrastructure.Data;
using OrderEntity = Ocow.Order.Domain.Models.Order;

namespace Ocow.Order.Infrastructure.Repositories;

/// <summary>
/// 订单仓储 EF Core 实现，用于持久化订单数据。
/// </summary>
public class OrderRepository : EfRepositoryBase<OrderDbContext, OrderEntity, Guid>, IOrderRepository
{
    public OrderRepository(OrderDbContext dbContext)
        : base(dbContext)
    {
    }

    /// <summary>
    /// 按订单编号查询订单。
    /// </summary>
    public override async Task<OrderEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    /// <summary>
    /// 按外部来源和外部订单编号查询订单，用于 ERP 同步幂等判断。
    /// </summary>
    public async Task<OrderEntity?> GetByExternalOrderAsync(string sourceSystem, string externalOrderId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.SourceSystem == sourceSystem && x.ExternalOrderId == externalOrderId, cancellationToken);
    }

    /// <summary>
    /// 分页查询指定会员的订单。
    /// </summary>
    public async Task<(IReadOnlyList<OrderEntity> Items, long Total)> GetCustomerOrdersAsync(Guid customerId, int pageIndex, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = DbSet
            .Where(x => x.CustomerId == customerId)
            .OrderByDescending(x => x.CreatedAt);

        var total = await query.LongCountAsync(cancellationToken);
        var items = await query.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        return (items, total);
    }

    /// <summary>
    /// 分页查询后台订单列表。
    /// </summary>
    public async Task<(IReadOnlyList<OrderEntity> Items, long Total)> GetAdminOrdersAsync(int pageIndex, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = DbSet.OrderByDescending(x => x.CreatedAt);
        var total = await query.LongCountAsync(cancellationToken);
        var items = await query.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        return (items, total);
    }
}
