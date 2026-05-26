using Microsoft.EntityFrameworkCore;
using Ocow.Inventory.Domain.Models;
using Ocow.Inventory.Infrastructure.Models;

namespace Ocow.Inventory.Infrastructure.Data;

/// <summary>
/// 库存服务数据库上下文，用于配置库存商品、库存锁和幂等表映射。
/// </summary>
public class InventoryDbContext : DbContext
{
    /// <summary>
    /// 创建库存服务数据库上下文。
    /// </summary>
    public InventoryDbContext(DbContextOptions<InventoryDbContext> options)
        : base(options)
    {
    }

    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();

    public DbSet<InventoryLock> InventoryLocks => Set<InventoryLock>();

    public DbSet<InventoryLockItem> InventoryLockItems => Set<InventoryLockItem>();

    public DbSet<ProcessedIntegrationEvent> ProcessedIntegrationEvents => Set<ProcessedIntegrationEvent>();

    /// <summary>
    /// 配置库存服务实体关系和唯一约束。
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<InventoryItem>(entity =>
        {
            entity.HasIndex(x => x.SkuId).IsUnique();
        });

        modelBuilder.Entity<InventoryLock>(entity =>
        {
            entity.HasIndex(x => x.OrderId).IsUnique();
            entity.HasMany(x => x.Items)
                .WithOne(x => x.InventoryLock)
                .HasForeignKey(x => x.InventoryLockId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProcessedIntegrationEvent>(entity =>
        {
            entity.Property(x => x.EventName)
                .HasMaxLength(256)
                .IsRequired();
        });
    }
}
