using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Ocow.EntityFrameworkCore.Extensions;
using Ocow.EntityFrameworkCore.Options;
using Ocow.Inventory.Infrastructure.Data;

namespace Ocow.Inventory.Migrations.Factories;

/// <summary>
/// 库存迁移上下文工厂，用于 EF Core CLI 生成迁移。
/// </summary>
public class InventoryDbContextFactory : IDesignTimeDbContextFactory<InventoryDbContext>
{
    /// <summary>
    /// 创建设计时库存数据库上下文。
    /// </summary>
    public InventoryDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<InventoryDbContext>();
        optionsBuilder.UseOcowDatabase(new DatabaseOption
        {
            Provider = DatabaseProviderEnum.PostgreSql,
            ConnectionString = "Host=localhost;Port=5432;Database=ocow_inventory;Username=postgres;Password=postgres123",
            MigrationsAssembly = typeof(InventoryDbContextFactory).Assembly.GetName().Name
        });

        return new InventoryDbContext(optionsBuilder.Options);
    }
}
