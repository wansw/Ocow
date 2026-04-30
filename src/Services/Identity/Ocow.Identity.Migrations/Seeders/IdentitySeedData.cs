namespace Ocow.Identity.Migrations.Seeders;

/// <summary>
/// 身份服务种子数据常量，用于统一维护默认管理员、角色和权限点标识。
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
        new(Guid.Parse("30000000-0000-0000-0000-000000000008"), "scheduler.trigger", "触发任务", "scheduler")
    ];
}
