using Microsoft.EntityFrameworkCore;
using Ocow.EntityFrameworkCore.Seeders;
using Ocow.Identity.Domain.Models;
using Ocow.Identity.Infrastructure.Data;

namespace Ocow.Identity.Migrations.Seeders;

/// <summary>
/// 身份服务角色播种器，用于幂等初始化超级管理员角色和权限绑定。
/// </summary>
public class IdentityRoleSeeder : IDataSeeder
{
    private readonly IdentityDbContext _dbContext;

    public IdentityRoleSeeder(IdentityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// 执行角色和角色权限种子数据初始化。
    /// </summary>
    public async Task<SeedExecutionResult> SeedAsync(CancellationToken cancellationToken = default)
    {
        var inserted = 0;
        var updated = 0;

        var role = await _dbContext.Roles.FirstOrDefaultAsync(x => x.Code == IdentitySeedData.SuperAdminRoleCode, cancellationToken);
        if (role is null)
        {
            role = new RoleModel
            {
                Id = IdentitySeedData.SuperAdminRoleId,
                Code = IdentitySeedData.SuperAdminRoleCode,
                Name = IdentitySeedData.SuperAdminRoleName
            };
            _dbContext.Roles.Add(role);
            inserted++;
        }
        else if (role.Name != IdentitySeedData.SuperAdminRoleName)
        {
            role.Name = IdentitySeedData.SuperAdminRoleName;
            updated++;
        }

        var permissionCodes = IdentitySeedData.Permissions.Select(x => x.Code).ToArray();
        var permissions = await _dbContext.Permissions
            .Where(x => permissionCodes.Contains(x.Code))
            .ToListAsync(cancellationToken);
        var permissionIds = permissions.Select(x => x.Id).ToArray();
        var existingBindings = await _dbContext.RolePermissions
            .Where(x => x.RoleId == role.Id && permissionIds.Contains(x.PermissionId))
            .ToListAsync(cancellationToken);

        foreach (var permissionId in permissionIds)
        {
            if (existingBindings.Any(x => x.PermissionId == permissionId))
            {
                continue;
            }

            _dbContext.RolePermissions.Add(new RolePermissionModel
            {
                RoleId = role.Id,
                PermissionId = permissionId
            });
            inserted++;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return SeedExecutionResult.Completed(nameof(IdentityRoleSeeder), inserted, updated);
    }
}
