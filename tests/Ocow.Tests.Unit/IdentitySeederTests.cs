using Microsoft.EntityFrameworkCore;
using Ocow.Identity.Infrastructure.Data;
using Ocow.Identity.Migrations.Seeders;

namespace Ocow.Tests.Unit;

/// <summary>
/// 身份服务种子数据测试，用于验证默认权限、角色和管理员初始化规则。
/// </summary>
public class IdentitySeederTests
{
    /// <summary>
    /// 验证 Identity 种子数据重复执行时不会产生重复记录。
    /// </summary>
    [Fact]
    public async Task RunAsync_WhenExecutedTwice_ShouldKeepSeedDataIdempotent()
    {
        var oldPassword = Environment.GetEnvironmentVariable("OCOW_IDENTITY_ADMIN_PASSWORD");
        var oldUserName = Environment.GetEnvironmentVariable("OCOW_IDENTITY_ADMIN_USERNAME");
        var oldDisplayName = Environment.GetEnvironmentVariable("OCOW_IDENTITY_ADMIN_DISPLAY_NAME");

        try
        {
            Environment.SetEnvironmentVariable("OCOW_IDENTITY_ADMIN_PASSWORD", "UnitTest_Admin_2026");
            Environment.SetEnvironmentVariable("OCOW_IDENTITY_ADMIN_USERNAME", "seed-admin");
            Environment.SetEnvironmentVariable("OCOW_IDENTITY_ADMIN_DISPLAY_NAME", "种子管理员");

            var options = new DbContextOptionsBuilder<IdentityDbContext>()
                .UseInMemoryDatabase($"identity-seed-{Guid.NewGuid():N}")
                .Options;

            await using var dbContext = new IdentityDbContext(options);
            var runner = new IdentitySeedRunner(dbContext);

            await runner.RunAsync();
            await runner.RunAsync();

            Assert.Equal(8, await dbContext.Permissions.CountAsync());
            Assert.Single(await dbContext.Roles.ToListAsync());
            Assert.Single(await dbContext.AdminUsers.ToListAsync());
            Assert.Equal(8, await dbContext.RolePermissions.CountAsync());
            Assert.Single(await dbContext.AdminUserRoles.ToListAsync());
        }
        finally
        {
            Environment.SetEnvironmentVariable("OCOW_IDENTITY_ADMIN_PASSWORD", oldPassword);
            Environment.SetEnvironmentVariable("OCOW_IDENTITY_ADMIN_USERNAME", oldUserName);
            Environment.SetEnvironmentVariable("OCOW_IDENTITY_ADMIN_DISPLAY_NAME", oldDisplayName);
        }
    }
}
