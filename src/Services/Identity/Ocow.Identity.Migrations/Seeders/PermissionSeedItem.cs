namespace Ocow.Identity.Migrations.Seeders;

/// <summary>
/// 权限点种子定义，用于描述需要初始化的权限基础信息。/// </summary>
internal sealed record PermissionSeedItem(Guid Id, string Code, string Name, string Module);
