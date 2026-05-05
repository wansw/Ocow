namespace Ocow.Shared.Dtos;

/// <summary>
/// 通用分页响应 DTO，用于返回列表数据和分页信息。/// </summary>
public class PageResDto<T>
{
    /// <summary>
    /// 当前页数据列表。    /// </summary>
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();

    /// <summary>
    /// 满足条件的数据总数。    /// </summary>
    public long Total { get; init; }

    /// <summary>
    /// 当前页码。    /// </summary>
    public int PageIndex { get; init; }

    /// <summary>
    /// 每页数量。    /// </summary>
    public int PageSize { get; init; }
}
