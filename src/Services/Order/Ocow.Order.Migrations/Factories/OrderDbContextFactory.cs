using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Ocow.EntityFrameworkCore.Extensions;
using Ocow.EntityFrameworkCore.Options;
using Ocow.Order.Infrastructure.Data;

namespace Ocow.Order.Migrations.Factories;

/// <summary>
/// 订单迁移上下文工厂，用于 EF Core CLI 生成迁移。
/// </summary>
public class OrderDbContextFactory : IDesignTimeDbContextFactory<OrderDbContext>
{
    /// <summary>
    /// 创建设计时订单数据库上下文。
    /// </summary>
    public OrderDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<OrderDbContext>();
        optionsBuilder.UseOcowDatabase(new DatabaseOption
        {
            Provider = DatabaseProviderEnum.PostgreSql,
            ConnectionString = "Host=localhost;Port=5432;Database=ocow_order;Username=postgres;Password=postgres123"
        });

        return new OrderDbContext(optionsBuilder.Options);
    }
}
