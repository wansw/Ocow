using Microsoft.EntityFrameworkCore;
using Ocow.Identity.Domain.Models;

namespace Ocow.Identity.Infrastructure.Data;

/// <summary>
/// 身份认证数据库上下文，用于配置管理员、角色、权限和 Token 表映射。
/// </summary>
public class IdentityDbContext : DbContext
{
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options)
        : base(options)
    {
    }

    public DbSet<AdminUserModel> AdminUsers => Set<AdminUserModel>();

    public DbSet<RoleModel> Roles => Set<RoleModel>();

    public DbSet<PermissionModel> Permissions => Set<PermissionModel>();

    public DbSet<AdminUserRoleModel> AdminUserRoles => Set<AdminUserRoleModel>();

    public DbSet<RolePermissionModel> RolePermissions => Set<RolePermissionModel>();

    public DbSet<MemberIdentityModel> MemberIdentities => Set<MemberIdentityModel>();

    public DbSet<RefreshTokenModel> RefreshTokens => Set<RefreshTokenModel>();

    public DbSet<LoginLogModel> LoginLogs => Set<LoginLogModel>();

    /// <summary>
    /// 配置实体特性无法清晰表达的身份服务索引和复合主键规则。
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AdminUserModel>()
            .HasIndex(x => x.UserName)
            .IsUnique();

        modelBuilder.Entity<RoleModel>()
            .HasIndex(x => x.Code)
            .IsUnique();

        modelBuilder.Entity<PermissionModel>()
            .HasIndex(x => x.Code)
            .IsUnique();

        modelBuilder.Entity<MemberIdentityModel>()
            .HasIndex(x => x.OpenId)
            .IsUnique();

        modelBuilder.Entity<RefreshTokenModel>()
            .HasIndex(x => x.Token)
            .IsUnique();

        modelBuilder.Entity<AdminUserRoleModel>(entity =>
        {
            entity.HasKey(x => new { x.AdminUserId, x.RoleId });
            entity.HasOne(x => x.AdminUser)
                .WithMany(x => x.AdminUserRoles)
                .HasForeignKey(x => x.AdminUserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Role)
                .WithMany(x => x.AdminUserRoles)
                .HasForeignKey(x => x.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RolePermissionModel>(entity =>
        {
            entity.HasKey(x => new { x.RoleId, x.PermissionId });
            entity.HasOne(x => x.Role)
                .WithMany(x => x.RolePermissions)
                .HasForeignKey(x => x.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Permission)
                .WithMany(x => x.RolePermissions)
                .HasForeignKey(x => x.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
