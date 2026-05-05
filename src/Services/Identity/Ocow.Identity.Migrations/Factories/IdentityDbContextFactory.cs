using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Ocow.EntityFrameworkCore.Extensions;
using Ocow.EntityFrameworkCore.Options;
using Ocow.Identity.Infrastructure.Data;

namespace Ocow.Identity.Migrations.Factories;

/// <summary>
/// 身份服务迁移上下文工厂，用于 EF Core CLI 生成迁移。/// </summary>
public class IdentityDbContextFactory : IDesignTimeDbContextFactory<IdentityDbContext>
{
    /// <summary>
    /// 创建设计时身份服务数据库上下文。    /// </summary>
    public IdentityDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<IdentityDbContext>();
        optionsBuilder.UseOcowDatabase(new DatabaseOption
        {
            Provider = DatabaseProviderEnum.PostgreSql,
            ConnectionString = "Host=localhost;Port=5432;Database=ocow_identity;Username=postgres;Password=postgres123",
            MigrationsAssembly = typeof(IdentityDbContextFactory).Assembly.GetName().Name
        });

        return new IdentityDbContext(optionsBuilder.Options);
    }
}
