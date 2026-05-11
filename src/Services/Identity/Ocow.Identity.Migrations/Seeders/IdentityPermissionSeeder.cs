using Microsoft.EntityFrameworkCore;
using Ocow.EntityFrameworkCore.Seeders;
using Ocow.Identity.Domain.Models;
using Ocow.Identity.Infrastructure.Data;

namespace Ocow.Identity.Migrations.Seeders;

/// <summary>
/// 身份服务权限点播种器，用于幂等初始化后台权限点
/// </summary>
public class IdentityPermissionSeeder : IDataSeeder
{
    private readonly IdentityDbContext _dbContext;

    public IdentityPermissionSeeder(IdentityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// 执行权限点种子数据初始化。    /// </summary>
    public async Task<SeedExecutionResult> SeedAsync(CancellationToken cancellationToken = default)
    {
        var codes = IdentitySeedData.Permissions.Select(x => x.Code).ToArray();
        var existingPermissions = await _dbContext.Permissions
            .Where(x => codes.Contains(x.Code))
            .ToListAsync(cancellationToken);

        var inserted = 0;
        var updated = 0;

        foreach (var permission in IdentitySeedData.Permissions)
        {
            var existing = existingPermissions.FirstOrDefault(x => x.Code == permission.Code);
            if (existing is null)
            {
                _dbContext.Permissions.Add(new Permission
                {
                    Id = permission.Id,
                    Code = permission.Code,
                    Name = permission.Name,
                    Module = permission.Module
                });
                inserted++;
                continue;
            }

            if (existing.Name == permission.Name && existing.Module == permission.Module)
            {
                continue;
            }

            existing.Name = permission.Name;
            existing.Module = permission.Module;
            updated++;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return SeedExecutionResult.Completed(nameof(IdentityPermissionSeeder), inserted, updated);
    }
}
