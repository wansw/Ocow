using Microsoft.EntityFrameworkCore;
using Ocow.EntityFrameworkCore.Seeders;
using Ocow.Identity.Application.Services;
using Ocow.Identity.Domain.Enums;
using Ocow.Identity.Domain.Models;
using Ocow.Identity.Infrastructure.Data;

namespace Ocow.Identity.Migrations.Seeders;

/// <summary>
/// 身份服务管理员播种器，用于从环境变量读取密码并幂等初始化默认管理员。
/// </summary>
public class IdentityAdminUserSeeder : IDataSeeder
{
    private const string AdminPasswordEnvName = "OCOW_IDENTITY_ADMIN_PASSWORD";
    private const string AdminUserNameEnvName = "OCOW_IDENTITY_ADMIN_USERNAME";
    private const string AdminDisplayNameEnvName = "OCOW_IDENTITY_ADMIN_DISPLAY_NAME";

    private readonly IdentityDbContext _dbContext;

    public IdentityAdminUserSeeder(IdentityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// 执行默认管理员和管理员角色绑定种子数据初始化。
    /// </summary>
    public async Task<SeedExecutionResult> SeedAsync(CancellationToken cancellationToken = default)
    {
        var password = Environment.GetEnvironmentVariable(AdminPasswordEnvName);
        if (string.IsNullOrWhiteSpace(password))
        {
            //return SeedExecutionResult.Completed(
            //    nameof(IdentityAdminUserSeeder),
            //    inserted: 0,
            //    updated: 0,
            //    skipped: 1,
            //    message: $"未设置 {AdminPasswordEnvName}，已跳过默认管理员初始化。");
            password = "admin";
        }
        Console.WriteLine($"password:{password}");
        var userName = Environment.GetEnvironmentVariable(AdminUserNameEnvName);
        if (string.IsNullOrWhiteSpace(userName))
        {
            userName = "admin";
        }

        var displayName = Environment.GetEnvironmentVariable(AdminDisplayNameEnvName);
        if (string.IsNullOrWhiteSpace(displayName))
        {
            displayName = "超级管理员";
        }

        var inserted = 0;
        var updated = 0;
        var passwordHash = PasswordHashService.Hash(password);

        var adminUser = await _dbContext.AdminUsers.FirstOrDefaultAsync(x => x.UserName == userName, cancellationToken);
        if (adminUser is null)
        {
            adminUser = new AdminUser
            {
                Id = IdentitySeedData.SuperAdminUserId,
                UserName = userName,
                DisplayName = displayName,
                PasswordHash = passwordHash,
                Status = AdminUserStatusEnum.Enabled
            };
            _dbContext.AdminUsers.Add(adminUser);
            inserted++;
        }
        else
        {
            if (adminUser.DisplayName != displayName)
            {
                adminUser.DisplayName = displayName;
                updated++;
            }

            if (adminUser.PasswordHash != passwordHash)
            {
                adminUser.PasswordHash = passwordHash;
                updated++;
            }

            if (adminUser.Status != AdminUserStatusEnum.Enabled)
            {
                adminUser.Status = AdminUserStatusEnum.Enabled;
                updated++;
            }
        }

        var roleId = await _dbContext.Roles
            .Where(x => x.Code == IdentitySeedData.SuperAdminRoleCode)
            .Select(x => x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (roleId == Guid.Empty)
        {
            return SeedExecutionResult.Completed(
                nameof(IdentityAdminUserSeeder),
                inserted,
                updated,
                skipped: 1,
                message: "未找到超级管理员角色，已跳过默认管理员角色绑定。");
        }

        var hasRoleBinding = await _dbContext.AdminUserRoles.AnyAsync(
            x => x.AdminUserId == adminUser.Id && x.RoleId == roleId,
            cancellationToken);

        if (!hasRoleBinding)
        {
            _dbContext.AdminUserRoles.Add(new AdminUserRole
            {
                AdminUserId = adminUser.Id,
                RoleId = roleId
            });
            inserted++;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return SeedExecutionResult.Completed(nameof(IdentityAdminUserSeeder), inserted, updated);
    }
}
