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
    /// 配置身份服务表结构。
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AdminUserModel>(entity =>
        {
            entity.ToTable("admin_users");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.UserName).HasMaxLength(64).IsRequired();
            entity.Property(x => x.PasswordHash).HasMaxLength(128).IsRequired();
            entity.Property(x => x.DisplayName).HasMaxLength(64).IsRequired();
            entity.HasIndex(x => x.UserName).IsUnique();
        });

        modelBuilder.Entity<RoleModel>(entity =>
        {
            entity.ToTable("roles");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Code).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(64).IsRequired();
            entity.HasIndex(x => x.Code).IsUnique();
        });

        modelBuilder.Entity<PermissionModel>(entity =>
        {
            entity.ToTable("permissions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Code).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Module).HasMaxLength(64).IsRequired();
            entity.HasIndex(x => x.Code).IsUnique();
        });

        modelBuilder.Entity<AdminUserRoleModel>(entity =>
        {
            entity.ToTable("admin_user_roles");
            entity.HasKey(x => new { x.AdminUserId, x.RoleId });
        });

        modelBuilder.Entity<RolePermissionModel>(entity =>
        {
            entity.ToTable("role_permissions");
            entity.HasKey(x => new { x.RoleId, x.PermissionId });
        });

        modelBuilder.Entity<MemberIdentityModel>(entity =>
        {
            entity.ToTable("member_identities");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.OpenId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.UnionId).HasMaxLength(128);
            entity.HasIndex(x => x.OpenId).IsUnique();
        });

        modelBuilder.Entity<RefreshTokenModel>(entity =>
        {
            entity.ToTable("refresh_tokens");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Token).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Scope).HasMaxLength(32).IsRequired();
            entity.HasIndex(x => x.Token).IsUnique();
        });

        modelBuilder.Entity<LoginLogModel>(entity =>
        {
            entity.ToTable("login_logs");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.LoginName).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Scope).HasMaxLength(32).IsRequired();
            entity.Property(x => x.FailureReason).HasMaxLength(256);
        });
    }
}
