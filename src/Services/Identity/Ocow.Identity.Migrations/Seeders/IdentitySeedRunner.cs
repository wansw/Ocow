using Ocow.EntityFrameworkCore.Seeders;
using Ocow.Identity.Infrastructure.Data;

namespace Ocow.Identity.Migrations.Seeders;

/// <summary>
/// 身份服务种子数据执行器，用于按权限、角色、管理员顺序初始化基础数据。/// </summary>
public class IdentitySeedRunner : IDataSeedRunner
{
    private readonly IReadOnlyList<IDataSeeder> _seeders;

    public IdentitySeedRunner(IdentityDbContext dbContext)
    {
        _seeders =
        [
            new IdentityPermissionSeeder(dbContext),
            new IdentityRoleSeeder(dbContext),
            new IdentityAdminUserSeeder(dbContext)
        ];
    }

    /// <summary>
    /// 运行身份服务全部种子数据初始化任务。    /// </summary>
    public async Task<IReadOnlyList<SeedExecutionResult>> RunAsync(CancellationToken cancellationToken = default)
    {
        var results = new List<SeedExecutionResult>();
        foreach (var seeder in _seeders)
        {
            results.Add(await seeder.SeedAsync(cancellationToken));
        }

        return results;
    }
}
