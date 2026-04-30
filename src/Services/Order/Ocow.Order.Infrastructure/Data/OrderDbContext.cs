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
    /// 配置订单表和订单明细表映射。
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OrderModel>(entity =>
        {
            entity.ToTable("orders");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.OrderNo).HasMaxLength(64).IsRequired();
            entity.Property(x => x.TotalAmount).HasPrecision(18, 2);
            entity.Property(x => x.ExpressCompany).HasMaxLength(64);
            entity.Property(x => x.ExpressNo).HasMaxLength(64);
            entity.HasMany(x => x.Items)
                .WithOne()
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OrderItemModel>(entity =>
        {
            entity.ToTable("order_items");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ProductName).HasMaxLength(128).IsRequired();
            entity.Property(x => x.UnitPrice).HasPrecision(18, 2);
        });
    }
}
