namespace Ocow.Identity.Infrastructure.Data;

/// <summary>
/// 身份服务种子数据，用于提供 MVP 默认管理员、角色和权限点。
/// </summary>
public static class IdentitySeedData
{
    public static readonly Guid SuperAdminUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid SuperAdminRoleId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    public static readonly IReadOnlyDictionary<string, Guid> PermissionIds = new Dictionary<string, Guid>
    {
        ["order.read"] = Guid.Parse("30000000-0000-0000-0000-000000000001"),
        ["order.ship"] = Guid.Parse("30000000-0000-0000-0000-000000000002"),
        ["order.close"] = Guid.Parse("30000000-0000-0000-0000-000000000003"),
        ["product.create"] = Guid.Parse("30000000-0000-0000-0000-000000000004"),
        ["product.update"] = Guid.Parse("30000000-0000-0000-0000-000000000005"),
        ["product.publish"] = Guid.Parse("30000000-0000-0000-0000-000000000006"),
        ["payment.refund"] = Guid.Parse("30000000-0000-0000-0000-000000000007"),
        ["scheduler.trigger"] = Guid.Parse("30000000-0000-0000-0000-000000000008")
    };
}
