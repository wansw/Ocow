using Ocow.Identity.Domain.Enums;

namespace Ocow.Identity.Migrations.Seeders;

/// <summary>
/// 菜单种子定义，用于描述需要初始化的后台菜单和按钮基础信息。
/// </summary>
internal sealed record MenuSeedItem(
    Guid Id,
    Guid? ParentId,
    string Code,
    string Name,
    MenuTypeEnum Type,
    string? Path,
    string? Component,
    string? Icon,
    int Sort,
    string? PermissionCode);
