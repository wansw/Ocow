using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Ocow.EntityFrameworkCore.Extensions;
using Ocow.EntityFrameworkCore.Options;

namespace Ocow.Tests.Unit;

/// <summary>
/// 数据库 Provider 单元测试，用于验证公共 EF Core 配置入口。/// </summary>
public class DatabaseProviderTests
{
    /// <summary>
    /// 验证 PostgreSQL Provider 可以通过统一扩展写入 DbContext 配置。    /// </summary>
    [Fact]
    public void UseOcowDatabase_WhenPostgreSql_ShouldConfigureProvider()
    {
        var builder = new DbContextOptionsBuilder();

        builder.UseOcowDatabase(new DatabaseOption
        {
            Provider = DatabaseProviderEnum.PostgreSql,
            ConnectionString = "Host=localhost;Port=5432;Database=ocow_test;Username=postgres;Password=postgres123"
        });

        var relationalExtension = builder.Options.Extensions.OfType<RelationalOptionsExtension>().FirstOrDefault();
        Assert.NotNull(relationalExtension);
        Assert.Equal("Host=localhost;Port=5432;Database=ocow_test;Username=postgres;Password=postgres123", relationalExtension.ConnectionString);
    }

    /// <summary>
    /// 验证空连接字符串会被拒绝。    /// </summary>
    [Fact]
    public void UseOcowDatabase_WhenConnectionStringEmpty_ShouldThrow()
    {
        var builder = new DbContextOptionsBuilder();

        Assert.Throws<InvalidOperationException>(() => builder.UseOcowDatabase(new DatabaseOption()));
    }
}
