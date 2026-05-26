using Microsoft.EntityFrameworkCore;
using Ocow.EntityFrameworkCore.Extensions;
using Ocow.EntityFrameworkCore.Options;
using Ocow.Jobs.Api.Data;

namespace Ocow.Jobs.Migrations;

/// <summary>
/// Jobs 服务数据库初始化入口，用于创建任务配置表和执行日志表。
/// </summary>
public static class Program
{
    private const string ServiceName = "JOBS";
    private const string DefaultConnectionString = "Host=localhost;Port=5432;Database=ocow_jobs;Username=postgres;Password=postgres123";

    /// <summary>
    /// 执行 Jobs 服务数据库初始化命令。
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
                return 0;
            }

            await using var dbContext = CreateDbContext();
            await InitializeDatabaseAsync(dbContext);

            Console.WriteLine("Jobs 数据库初始化完成。");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Jobs 数据库初始化失败：{ex.Message}");
            return 1;
        }
    }

    /// <summary>
    /// 创建 Jobs 服务数据库上下文。
    /// </summary>
    private static JobsDbContext CreateDbContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<JobsDbContext>();
        optionsBuilder.UseOcowDatabase(CreateDatabaseOption());
        return new JobsDbContext(optionsBuilder.Options);
    }

    /// <summary>
    /// 初始化数据库结构，存在迁移时执行迁移，否则按当前模型建表。
    /// </summary>
    private static async Task InitializeDatabaseAsync(JobsDbContext dbContext)
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
    /// 创建数据库连接配置，优先读取服务级环境变量。
    /// </summary>
    private static DatabaseOption CreateDatabaseOption()
    {
        return new DatabaseOption
        {
            Provider = ResolveProvider(GetEnvironmentValue(ServiceName, "DB_PROVIDER") ?? "PostgreSql"),
            ConnectionString = GetEnvironmentValue(ServiceName, "DB_CONNECTION_STRING") ?? DefaultConnectionString,
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
    /// 读取环境变量，支持服务级和全局两种命名。
    /// </summary>
    private static string? GetEnvironmentValue(string serviceName, string key)
    {
        return Environment.GetEnvironmentVariable($"OCOW_{serviceName}_{key}")
            ?? Environment.GetEnvironmentVariable($"OCOW_{key}");
    }

    /// <summary>
    /// 输出 Jobs 服务数据库初始化命令说明。
    /// </summary>
    private static void PrintUsage()
    {
        Console.WriteLine("用法：dotnet run --project src/Services/Jobs/Ocow.Jobs.Migrations/Ocow.Jobs.Migrations.csproj -- init");
        Console.WriteLine("环境变量：OCOW_JOBS_DB_PROVIDER、OCOW_JOBS_DB_CONNECTION_STRING");
    }
}
