namespace Ocow.EntityFrameworkCore.Abstractions;

/// <summary>
/// 审计实体接口，用于在保存时自动维护创建和更新时间。/// </summary>
public interface IAuditableEntity
{
    DateTime CreatedAt { get; set; }

    DateTime? UpdatedAt { get; set; }
}
