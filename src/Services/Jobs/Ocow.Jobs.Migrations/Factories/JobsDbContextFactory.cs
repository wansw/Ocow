using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Ocow.EntityFrameworkCore.Extensions;
using Ocow.EntityFrameworkCore.Options;
using Ocow.Jobs.Api.Data;

namespace Ocow.Jobs.Migrations.Factories;

/// <summary>
/// Jobs 迁移上下文工厂，用于 EF Core CLI 生成迁移。
/// </summary>
public class JobsDbContextFactory : IDesignTimeDbContextFactory<JobsDbContext>
{
    /// <summary>
    /// 创建设计时 Jobs 数据库上下文。
    /// </summary>
    public JobsDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<JobsDbContext>();
        optionsBuilder.UseOcowDatabase(new DatabaseOption
        {
            Provider = DatabaseProviderEnum.PostgreSql,
            ConnectionString = "Host=localhost;Port=5432;Database=ocow_jobs;Username=postgres;Password=postgres123",
            MigrationsAssembly = typeof(JobsDbContextFactory).Assembly.GetName().Name
        });

        return new JobsDbContext(optionsBuilder.Options);
    }
}
