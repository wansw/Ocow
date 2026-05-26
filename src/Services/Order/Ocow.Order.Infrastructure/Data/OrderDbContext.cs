using Microsoft.EntityFrameworkCore;
using Ocow.Order.Domain.Models;
using Ocow.Order.Infrastructure.Models;
using OrderEntity = Ocow.Order.Domain.Models.Order;

namespace Ocow.Order.Infrastructure.Data;

/// <summary>
/// 订单服务数据库上下文，用于配置订单聚合和集成事件幂等表的 EF Core 映射。
/// </summary>
public class OrderDbContext : DbContext
{
    /// <summary>
    /// 创建订单服务数据库上下文。
    /// </summary>
    public OrderDbContext(DbContextOptions<OrderDbContext> options)
        : base(options)
    {
    }

    public DbSet<OrderEntity> Orders => Set<OrderEntity>();

    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    public DbSet<ProcessedIntegrationEvent> ProcessedIntegrationEvents => Set<ProcessedIntegrationEvent>();

    /// <summary>
    /// 配置实体特性无法清晰表达的订单关系规则。
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OrderEntity>(entity =>
        {
            entity.HasMany(x => x.Items)
                .WithOne(x => x.Order)
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(x => new { x.SourceSystem, x.ExternalOrderId })
                .IsUnique();
        });

        modelBuilder.Entity<ProcessedIntegrationEvent>(entity =>
        {
            entity.Property(x => x.EventName)
                .HasMaxLength(256)
                .IsRequired();
        });
    }
}
