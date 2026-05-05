namespace Ocow.Shared.Dtos;

/// <summary>
/// 通用分页请求 DTO，用于列表查询入口。/// </summary>
public class PageReqDto
{
    /// <summary>
    /// 当前页码，从 1 开始。    /// </summary>
    public int PageIndex { get; init; } = 1;

    /// <summary>
    /// 每页数量，最大值由服务端保护。    /// </summary>
    public int PageSize { get; init; } = 20;

    /// <summary>
    /// 获取经过边界修正后的页码。    /// </summary>
    public int GetSafePageIndex()
    {
        return PageIndex < 1 ? 1 : PageIndex;
    }

    /// <summary>
    /// 获取经过边界修正后的每页数量。    /// </summary>
    public int GetSafePageSize()
    {
        return PageSize is < 1 or > 100 ? 20 : PageSize;
    }
}
