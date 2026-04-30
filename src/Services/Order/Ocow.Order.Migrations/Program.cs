using Microsoft.EntityFrameworkCore;
using Ocow.EntityFrameworkCore.Extensions;
using Ocow.EntityFrameworkCore.Options;
using Ocow.Order.Infrastructure.Data;

namespace Ocow.Order.Migrations;

/// <summary>
/// 订单服务数据库初始化入口，用于执行迁移并创建订单服务数据库结构。
/// </summary>
public static class Program
{
    private const string ServiceName = "ORDER";
    private const string DefaultConnectionString = "Host=localhost;Port=5432;Database=ocow_order;Username=postgres;Password=postgres123";

    /// <summary>
    /// 执行订单服务数据库初始化命令。
    /// </summary>
    public static async Task<int> Main(string[] args)
    {
        try
        {
            if (args.Contains("--help", StringComparer.OrdinalIgnoreCase))
            {
                PrintUsage();
                return 0;
            }

            await using var dbContext = CreateDbContext();
            await InitializeDatabaseAsync(dbContext);

            Console.WriteLine("Order 数据库初始化完成。");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Order 数据库初始化失败：{ex.Message}");
            return 1;
        }
    }

    /// <summary>
    /// 创建订单服务数据库上下文。
    /// </summary>
    private static OrderDbContext CreateDbContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<OrderDbContext>();
        optionsBuilder.UseOcowDatabase(CreateDatabaseOption(ServiceName, DefaultConnectionString));
        return new OrderDbContext(optionsBuilder.Options);
    }

    /// <summary>
    /// 初始化数据库结构；存在迁移时执行迁移，否则按当前模型创建数据库。
    /// </summary>
    private static async Task InitializeDatabaseAsync(OrderDbContext dbContext)
    {
        var migrations = dbContext.Database.GetMigrations();
        if (migrations.Any())
        {
            await dbContext.Database.MigrateAsync();
            return;
        }

        await dbContext.Database.EnsureCreatedAsync();
    }

    /// <summary>
    /// 创建数据库连接配置，优先读取服务级环境变量，其次读取全局环境变量。
    /// </summary>
    private static DatabaseOption CreateDatabaseOption(string serviceName, string defaultConnectionString)
    {
        return new DatabaseOption
        {
            Provider = ResolveProvider(GetEnvironmentValue(serviceName, "DB_PROVIDER") ?? "PostgreSql"),
            ConnectionString = GetEnvironmentValue(serviceName, "DB_CONNECTION_STRING") ?? defaultConnectionString
        };
    }

    /// <summary>
    /// 解析数据库 Provider 枚举。
    /// </summary>
    private static DatabaseProviderEnum ResolveProvider(string provider)
    {
        return Enum.TryParse<DatabaseProviderEnum>(provider, ignoreCase: true, out var value)
            ? value
            : DatabaseProviderEnum.PostgreSql;
    }

    /// <summary>
    /// 读取环境变量，支持 OCOW_{服务名}_{键} 和 OCOW_{键} 两种命名。
    /// </summary>
    private static string? GetEnvironmentValue(string serviceName, string key)
    {
        return Environment.GetEnvironmentVariable($"OCOW_{serviceName}_{key}")
            ?? Environment.GetEnvironmentVariable($"OCOW_{key}");
    }

    /// <summary>
    /// 输出订单服务数据库初始化命令说明。
    /// </summary>
    private static void PrintUsage()
    {
        Console.WriteLine("用法：dotnet run --project src/Services/Order/Ocow.Order.Migrations/Ocow.Order.Migrations.csproj");
        Console.WriteLine("环境变量：OCOW_ORDER_DB_PROVIDER、OCOW_ORDER_DB_CONNECTION_STRING");
    }
}
