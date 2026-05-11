using Ocow.Identity.Domain.Enums;

namespace Ocow.Identity.Migrations.Seeders;

/// <summary>
/// 身份服务种子数据常量，用于统一维护默认管理员、角色、权限点和菜单标识。
/// </summary>
internal static class IdentitySeedData
{
    public const string SuperAdminRoleCode = "super_admin";
    public const string SuperAdminRoleName = "超级管理员";

    public static readonly Guid SuperAdminUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid SuperAdminRoleId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    public static readonly IReadOnlyList<PermissionSeedItem> Permissions =
    [
        new(Guid.Parse("30000000-0000-0000-0000-000000000001"), "order.read", "查看订单", "order"),
        new(Guid.Parse("30000000-0000-0000-0000-000000000002"), "order.ship", "订单发货", "order"),
        new(Guid.Parse("30000000-0000-0000-0000-000000000003"), "order.close", "关闭订单", "order"),
        new(Guid.Parse("30000000-0000-0000-0000-000000000004"), "product.create", "创建商品", "product"),
        new(Guid.Parse("30000000-0000-0000-0000-000000000005"), "product.update", "修改商品", "product"),
        new(Guid.Parse("30000000-0000-0000-0000-000000000006"), "product.publish", "上下架商品", "product"),
        new(Guid.Parse("30000000-0000-0000-0000-000000000007"), "payment.refund", "支付退款", "payment"),
        new(Guid.Parse("30000000-0000-0000-0000-000000000008"), "scheduler.trigger", "触发任务", "scheduler"),
        new(Guid.Parse("30000000-0000-0000-0000-000000000101"), "identity.role.read", "查看角色", "identity"),
        new(Guid.Parse("30000000-0000-0000-0000-000000000102"), "identity.role.save", "保存角色", "identity"),
        new(Guid.Parse("30000000-0000-0000-0000-000000000103"), "identity.role.bind-permission", "绑定角色权限点", "identity"),
        new(Guid.Parse("30000000-0000-0000-0000-000000000105"), "identity.permission.read", "查看权限点", "identity"),
        new(Guid.Parse("30000000-0000-0000-0000-000000000106"), "identity.menu.read", "查看菜单", "identity"),
        new(Guid.Parse("30000000-0000-0000-0000-000000000107"), "identity.menu.save", "保存菜单", "identity"),
        new(Guid.Parse("30000000-0000-0000-0000-000000000108"), "identity.admin-user.read", "查看管理员", "identity"),
        new(Guid.Parse("30000000-0000-0000-0000-000000000109"), "identity.admin-user.create", "创建管理员", "identity"),
        new(Guid.Parse("30000000-0000-0000-0000-000000000110"), "identity.admin-user.disable", "禁用管理员", "identity")
    ];

    public static readonly IReadOnlyList<MenuSeedItem> Menus =
    [
        new(Guid.Parse("40000000-0000-0000-0000-000000000001"), null, "identity", "权限中心", MenuTypeEnum.Page, "/identity", "Layout", "shield", 10, null),
        new(Guid.Parse("40000000-0000-0000-0000-000000000002"), Guid.Parse("40000000-0000-0000-0000-000000000001"), "identity.roles", "角色管理", MenuTypeEnum.Page, "/identity/roles", "identity/roles/index", "users", 10, "identity.role.read"),
        new(Guid.Parse("40000000-0000-0000-0000-000000000003"), Guid.Parse("40000000-0000-0000-0000-000000000002"), "identity.roles.save", "保存角色", MenuTypeEnum.Button, null, null, null, 10, "identity.role.save"),
        new(Guid.Parse("40000000-0000-0000-0000-000000000004"), Guid.Parse("40000000-0000-0000-0000-000000000002"), "identity.roles.bind-permission", "绑定权限点", MenuTypeEnum.Button, null, null, null, 20, "identity.role.bind-permission"),
        new(Guid.Parse("40000000-0000-0000-0000-000000000006"), Guid.Parse("40000000-0000-0000-0000-000000000001"), "identity.permissions", "权限点管理", MenuTypeEnum.Page, "/identity/permissions", "identity/permissions/index", "key-round", 20, "identity.permission.read"),
        new(Guid.Parse("40000000-0000-0000-0000-000000000007"), Guid.Parse("40000000-0000-0000-0000-000000000001"), "identity.menus", "菜单管理", MenuTypeEnum.Page, "/identity/menus", "identity/menus/index", "menu", 30, "identity.menu.read"),
        new(Guid.Parse("40000000-0000-0000-0000-000000000008"), Guid.Parse("40000000-0000-0000-0000-000000000007"), "identity.menus.save", "保存菜单", MenuTypeEnum.Button, null, null, null, 10, "identity.menu.save"),
        new(Guid.Parse("40000000-0000-0000-0000-000000000009"), Guid.Parse("40000000-0000-0000-0000-000000000001"), "identity.admin-users", "管理员管理", MenuTypeEnum.Page, "/identity/admin-users", "identity/admin-users/index", "user-cog", 40, "identity.admin-user.read"),
        new(Guid.Parse("40000000-0000-0000-0000-000000000010"), Guid.Parse("40000000-0000-0000-0000-000000000009"), "identity.admin-users.create", "创建管理员", MenuTypeEnum.Button, null, null, null, 10, "identity.admin-user.create"),
        new(Guid.Parse("40000000-0000-0000-0000-000000000011"), Guid.Parse("40000000-0000-0000-0000-000000000009"), "identity.admin-users.disable", "禁用管理员", MenuTypeEnum.Button, null, null, null, 20, "identity.admin-user.disable")
    ];
}
