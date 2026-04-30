using Microsoft.EntityFrameworkCore;
using Ocow.Order.Application.Interfaces;
using Ocow.Order.Domain.Models;
using Ocow.Order.Infrastructure.Data;

namespace Ocow.Order.Infrastructure.Repositories;

/// <summary>
/// 订单仓储 EF Core 实现，用于持久化订单数据。
/// </summary>
public class OrderRepository : IOrderRepository
{
    private readonly OrderDbContext _dbContext;

    public OrderRepository(OrderDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// 新增订单。
    /// </summary>
    public async Task AddAsync(OrderModel order, CancellationToken cancellationToken = default)
    {
        await _dbContext.Orders.AddAsync(order, cancellationToken);
    }

    /// <summary>
    /// 按订单编号查询订单。
    /// </summary>
    public async Task<OrderModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Orders
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    /// <summary>
    /// 分页查询指定会员的订单。
    /// </summary>
    public async Task<(IReadOnlyList<OrderModel> Items, long Total)> GetCustomerOrdersAsync(Guid customerId, int pageIndex, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Orders
            .Where(x => x.CustomerId == customerId)
            .OrderByDescending(x => x.CreatedAt);

        var total = await query.LongCountAsync(cancellationToken);
        var items = await query.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        return (items, total);
    }

    /// <summary>
    /// 分页查询后台订单列表。
    /// </summary>
    public async Task<(IReadOnlyList<OrderModel> Items, long Total)> GetAdminOrdersAsync(int pageIndex, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Orders.OrderByDescending(x => x.CreatedAt);
        var total = await query.LongCountAsync(cancellationToken);
        var items = await query.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        return (items, total);
    }

    /// <summary>
    /// 保存订单状态变更。
    /// </summary>
    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
