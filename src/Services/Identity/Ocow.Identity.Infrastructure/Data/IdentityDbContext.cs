using Microsoft.EntityFrameworkCore;
using Ocow.Identity.Domain.Models;

namespace Ocow.Identity.Infrastructure.Data;

/// <summary>
/// 身份认证数据库上下文，用于配置管理员、角色、权限和 Token 表映射。/// </summary>
public class IdentityDbContext : DbContext
{
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options)
        : base(options)
    {
    }

    public DbSet<AdminUser> AdminUsers => Set<AdminUser>();

    public DbSet<Role> Roles => Set<Role>();

    public DbSet<Permission> Permissions => Set<Permission>();

    public DbSet<AdminUserRole> AdminUserRoles => Set<AdminUserRole>();

    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    public DbSet<MemberIdentity> MemberIdentities => Set<MemberIdentity>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<LoginLog> LoginLogs => Set<LoginLog>();

    /// <summary>
    /// 配置实体特性无法清晰表达的身份服务索引和复合主键规则。    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AdminUser>()
            .HasIndex(x => x.UserName)
            .IsUnique();

        modelBuilder.Entity<Role>()
            .HasIndex(x => x.Code)
            .IsUnique();

        modelBuilder.Entity<Permission>()
            .HasIndex(x => x.Code)
            .IsUnique();

        modelBuilder.Entity<MemberIdentity>()
            .HasIndex(x => x.OpenId)
            .IsUnique();

        modelBuilder.Entity<RefreshToken>()
            .HasIndex(x => x.Token)
            .IsUnique();

        modelBuilder.Entity<AdminUserRole>(entity =>
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

        modelBuilder.Entity<RolePermission>(entity =>
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
