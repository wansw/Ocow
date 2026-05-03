using Microsoft.EntityFrameworkCore;
using Ocow.Order.Domain.Models;

namespace Ocow.Order.Infrastructure.Data;

/// <summary>
/// 订单服务数据库上下文，用于配置订单聚合的 EF Core 映射。
/// </summary>
public class OrderDbContext : DbContext
{
    public OrderDbContext(DbContextOptions<OrderDbContext> options)
        : base(options)
    {
    }

    public DbSet<OrderModel> Orders => Set<OrderModel>();

    public DbSet<OrderItemModel> OrderItems => Set<OrderItemModel>();

    /// <summary>
    /// 配置实体特性无法清晰表达的订单关系规则。
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OrderModel>(entity =>
        {
            entity.HasMany(x => x.Items)
                .WithOne(x => x.Order)
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
