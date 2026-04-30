namespace Ocow.Shared.Dtos;

/// <summary>
/// 通用分页响应 DTO，用于返回列表数据和分页信息。
/// </summary>
public class PageResDto<T>
{
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();

    public long Total { get; init; }

    public int PageIndex { get; init; }

    public int PageSize { get; init; }
}
