using Microsoft.EntityFrameworkCore;
using Ocow.EntityFrameworkCore.Extensions;
using Ocow.EntityFrameworkCore.Options;
using Ocow.Identity.Infrastructure.Data;
using Ocow.Identity.Migrations.Seeders;

namespace Ocow.Identity.Migrations;

/// <summary>
/// 身份服务数据库初始化入口，用于执行迁移并初始化基础权限、角色和管理员数据。
/// </summary>
public static class Program
{
    private const string ServiceName = "IDENTITY";
    private static string DefaultConnectionString = "Host=localhost;Port=5432;Database=ocow_identity_dev;Username=postgres;Password=postgres123";

    /// <summary>
    /// 执行身份服务数据库初始化命令。
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

            if (!args.Contains("init", StringComparer.OrdinalIgnoreCase))
            {
                Console.WriteLine("请输入 --init");
                return 0;
            }

            Console.WriteLine("--init开始-----------------");

            if (args.Contains("env=pro", StringComparer.OrdinalIgnoreCase))
            {
                DefaultConnectionString = "Host=localhost;Port=5432;Database=ocow_identity;Username=postgres;Password=postgres123";
            }

            await using var dbContext = CreateDbContext();
            await InitializeDatabaseAsync(dbContext);

            if (!args.Contains("--no-seed", StringComparer.OrdinalIgnoreCase))
            {
                var seedResults = await new IdentitySeedRunner(dbContext).RunAsync();
                foreach (var result in seedResults)
                {
                    Console.WriteLine($"{result.SeederName}: inserted={result.Inserted}, updated={result.Updated}, skipped={result.Skipped}, message={result.Message}");
                }
            }

            Console.WriteLine("Identity 数据库初始化完成。");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Identity 数据库初始化失败：{ex.Message}");
            return 1;
        }
    }

    /// <summary>
    /// 创建身份服务数据库上下文。
    /// </summary>
    private static IdentityDbContext CreateDbContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<IdentityDbContext>();
        optionsBuilder.UseOcowDatabase(CreateDatabaseOption(ServiceName, DefaultConnectionString));
        return new IdentityDbContext(optionsBuilder.Options);
    }

    /// <summary>
    /// 初始化数据库结构；存在迁移时执行迁移，否则按当前模型创建数据库。
    /// </summary>
    private static async Task InitializeDatabaseAsync(IdentityDbContext dbContext)
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
            ConnectionString = GetEnvironmentValue(serviceName, "DB_CONNECTION_STRING") ?? defaultConnectionString,
            MigrationsAssembly = typeof(Program).Assembly.GetName().Name
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
    /// 输出身份服务数据库初始化命令说明。
    /// </summary>
    private static void PrintUsage()
    {
        Console.WriteLine("用法：dotnet run --project src/Services/Identity/Ocow.Identity.Migrations/Ocow.Identity.Migrations.csproj -- init [--no-seed]");
        Console.WriteLine("环境变量：OCOW_IDENTITY_DB_PROVIDER、OCOW_IDENTITY_DB_CONNECTION_STRING、OCOW_IDENTITY_ADMIN_PASSWORD");
        Console.WriteLine("可选环境变量：OCOW_IDENTITY_ADMIN_USERNAME、OCOW_IDENTITY_ADMIN_DISPLAY_NAME");
    }
}
