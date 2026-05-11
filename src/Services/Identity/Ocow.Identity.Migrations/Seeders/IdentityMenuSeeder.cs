using Microsoft.EntityFrameworkCore;
using Ocow.EntityFrameworkCore.Seeders;
using Ocow.Identity.Domain.Models;
using Ocow.Identity.Infrastructure.Data;

namespace Ocow.Identity.Migrations.Seeders;

/// <summary>
/// 身份服务菜单播种器，用于幂等初始化 PC 后台菜单和菜单权限点关系。
/// </summary>
public class IdentityMenuSeeder : IDataSeeder
{
    private readonly IdentityDbContext _dbContext;

    public IdentityMenuSeeder(IdentityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// 执行后台菜单种子数据初始化。
    /// </summary>
    public async Task<SeedExecutionResult> SeedAsync(CancellationToken cancellationToken = default)
    {
        var inserted = 0;
        var updated = 0;

        var permissionCodes = IdentitySeedData.Menus
            .Where(x => !string.IsNullOrWhiteSpace(x.PermissionCode))
            .Select(x => x.PermissionCode!)
            .Distinct()
            .ToArray();
        var permissionMap = await _dbContext.Permissions
            .Where(x => permissionCodes.Contains(x.Code))
            .ToDictionaryAsync(x => x.Code, x => x.Id, cancellationToken);

        var menuCodes = IdentitySeedData.Menus.Select(x => x.Code).ToArray();
        var existingMenus = await _dbContext.Menus
            .Where(x => menuCodes.Contains(x.Code))
            .ToListAsync(cancellationToken);

        foreach (var seedMenu in IdentitySeedData.Menus)
        {
            var permissionId = seedMenu.PermissionCode is not null && permissionMap.TryGetValue(seedMenu.PermissionCode, out var foundPermissionId)
                ? foundPermissionId
                : (Guid?)null;
            var existing = existingMenus.FirstOrDefault(x => x.Code == seedMenu.Code);
            if (existing is null)
            {
                _dbContext.Menus.Add(new Menu
                {
                    Id = seedMenu.Id,
                    ParentId = seedMenu.ParentId,
                    Code = seedMenu.Code,
                    Name = seedMenu.Name,
                    Type = seedMenu.Type,
                    Path = seedMenu.Path,
                    Component = seedMenu.Component,
                    Icon = seedMenu.Icon,
                    Sort = seedMenu.Sort,
                    PermissionId = permissionId
                });
                inserted++;
                continue;
            }

            if (existing.ParentId == seedMenu.ParentId &&
                existing.Name == seedMenu.Name &&
                existing.Type == seedMenu.Type &&
                existing.Path == seedMenu.Path &&
                existing.Component == seedMenu.Component &&
                existing.Icon == seedMenu.Icon &&
                existing.Sort == seedMenu.Sort &&
                existing.PermissionId == permissionId)
            {
                continue;
            }

            existing.ParentId = seedMenu.ParentId;
            existing.Name = seedMenu.Name;
            existing.Type = seedMenu.Type;
            existing.Path = seedMenu.Path;
            existing.Component = seedMenu.Component;
            existing.Icon = seedMenu.Icon;
            existing.Sort = seedMenu.Sort;
            existing.PermissionId = permissionId;
            updated++;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return SeedExecutionResult.Completed(nameof(IdentityMenuSeeder), inserted, updated);
    }
}
