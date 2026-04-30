namespace Ocow.EntityFrameworkCore.Abstractions;

/// <summary>
/// 软删除接口，用于将删除操作转换为标记删除。
/// </summary>
public interface ISoftDelete
{
    bool IsDeleted { get; set; }

    DateTime? DeletedAt { get; set; }
}
