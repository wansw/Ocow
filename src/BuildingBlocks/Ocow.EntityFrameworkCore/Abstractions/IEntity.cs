namespace Ocow.EntityFrameworkCore.Abstractions;

/// <summary>
/// 实体基础接口，用于统一表达实体主键。
/// </summary>
public interface IEntity<TKey>
{
    TKey Id { get; set; }
}
