using Microsoft.EntityFrameworkCore;
using Ocow.Identity.Application.Services;
using Ocow.Identity.Domain.Enums;
using Ocow.Identity.Domain.Models;

namespace Ocow.Identity.Infrastructure.Data;

/// <summary>
/// 身份认证数据库上下文，用于配置管理员、角色、权限和 Token 映射。
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
    /// 配置身份服务表结构和 MVP 种子数据。
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

        Seed(modelBuilder);
    }

    /// <summary>
    /// 写入默认超级管理员、角色和权限点种子数据。
    /// </summary>
    private static void Seed(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AdminUserModel>().HasData(new AdminUserModel
        {
            Id = IdentitySeedData.SuperAdminUserId,
            UserName = "admin",
            DisplayName = "超级管理员",
            PasswordHash = PasswordHashService.Hash("Ocow@2026"),
            Status = AdminUserStatusEnum.Enabled,
            CreatedAt = new DateTime(2026, 4, 30, 0, 0, 0, DateTimeKind.Utc)
        });

        modelBuilder.Entity<RoleModel>().HasData(new RoleModel
        {
            Id = IdentitySeedData.SuperAdminRoleId,
            Code = "super_admin",
            Name = "超级管理员",
            CreatedAt = new DateTime(2026, 4, 30, 0, 0, 0, DateTimeKind.Utc)
        });

        modelBuilder.Entity<AdminUserRoleModel>().HasData(new AdminUserRoleModel
        {
            AdminUserId = IdentitySeedData.SuperAdminUserId,
            RoleId = IdentitySeedData.SuperAdminRoleId
        });

        var permissions = new[]
        {
            new PermissionModel { Id = IdentitySeedData.PermissionIds["order.read"], Code = "order.read", Name = "查看订单", Module = "order" },
            new PermissionModel { Id = IdentitySeedData.PermissionIds["order.ship"], Code = "order.ship", Name = "订单发货", Module = "order" },
            new PermissionModel { Id = IdentitySeedData.PermissionIds["order.close"], Code = "order.close", Name = "关闭订单", Module = "order" },
            new PermissionModel { Id = IdentitySeedData.PermissionIds["product.create"], Code = "product.create", Name = "创建商品", Module = "product" },
            new PermissionModel { Id = IdentitySeedData.PermissionIds["product.update"], Code = "product.update", Name = "修改商品", Module = "product" },
            new PermissionModel { Id = IdentitySeedData.PermissionIds["product.publish"], Code = "product.publish", Name = "上下架商品", Module = "product" },
            new PermissionModel { Id = IdentitySeedData.PermissionIds["payment.refund"], Code = "payment.refund", Name = "支付退款", Module = "payment" },
            new PermissionModel { Id = IdentitySeedData.PermissionIds["scheduler.trigger"], Code = "scheduler.trigger", Name = "触发任务", Module = "scheduler" }
        };

        modelBuilder.Entity<PermissionModel>().HasData(permissions);
        modelBuilder.Entity<RolePermissionModel>().HasData(permissions.Select(x => new RolePermissionModel
        {
            RoleId = IdentitySeedData.SuperAdminRoleId,
            PermissionId = x.Id
        }));
    }
}
