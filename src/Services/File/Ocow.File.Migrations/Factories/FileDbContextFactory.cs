using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Ocow.EntityFrameworkCore.Extensions;
using Ocow.EntityFrameworkCore.Options;
using Ocow.Files.Infrastructure.Data;

namespace Ocow.Files.Migrations.Factories;

/// <summary>
/// 文件服务迁移上下文工厂，用于 EF Core CLI 生成迁移。
/// </summary>
public class FileDbContextFactory : IDesignTimeDbContextFactory<FileDbContext>
{
    /// <summary>
    /// 创建设计时文件服务数据库上下文。
    /// </summary>
    public FileDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<FileDbContext>();
        optionsBuilder.UseOcowDatabase(new DatabaseOption
        {
            Provider = DatabaseProviderEnum.PostgreSql,
            ConnectionString = "Host=localhost;Port=5432;Database=ocow_file;Username=postgres;Password=postgres123",
            MigrationsAssembly = typeof(FileDbContextFactory).Assembly.GetName().Name
        });

        return new FileDbContext(optionsBuilder.Options);
    }
}
