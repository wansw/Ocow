using Microsoft.EntityFrameworkCore;
using Ocow.EntityFrameworkCore.Extensions;
using Ocow.EntityFrameworkCore.Options;
using Ocow.Files.Infrastructure.Data;

namespace Ocow.Files.Migrations;

/// <summary>
/// 文件服务数据库初始化入口，用于执行迁移或创建文件元数据表。
/// </summary>
public static class Program
{
    private const string ServiceName = "FILE";
    private static string DefaultConnectionString = "Host=localhost;Port=5432;Database=ocow_file_dev;Username=postgres;Password=postgres123";

    /// <summary>
    /// 执行文件服务数据库初始化命令。
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
                Console.WriteLine("请输入 init");
                return 0;
            }

            if (args.Contains("env=pro", StringComparer.OrdinalIgnoreCase))
            {
                DefaultConnectionString = "Host=localhost;Port=5432;Database=ocow_file;Username=postgres;Password=postgres123";
            }

            await using var dbContext = CreateDbContext();
            await InitializeDatabaseAsync(dbContext);

            Console.WriteLine("File 数据库初始化完成。");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"File 数据库初始化失败：{ex.Message}");
            return 1;
        }
    }

    /// <summary>
    /// 创建文件服务数据库上下文。
    /// </summary>
    private static FileDbContext CreateDbContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<FileDbContext>();
        optionsBuilder.UseOcowDatabase(CreateDatabaseOption(ServiceName, DefaultConnectionString));
        return new FileDbContext(optionsBuilder.Options);
    }

    /// <summary>
    /// 初始化数据库结构；存在迁移时执行迁移，否则按当前模型创建数据库。
    /// </summary>
    private static async Task InitializeDatabaseAsync(FileDbContext dbContext)
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
    /// 输出文件服务数据库初始化命令说明。
    /// </summary>
    private static void PrintUsage()
    {
        Console.WriteLine("用法：dotnet run --project src/Services/File/Ocow.File.Migrations/Ocow.File.Migrations.csproj -- init");
        Console.WriteLine("环境变量：OCOW_FILE_DB_PROVIDER、OCOW_FILE_DB_CONNECTION_STRING");
    }
}
