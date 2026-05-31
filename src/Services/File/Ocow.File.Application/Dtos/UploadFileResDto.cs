namespace Ocow.Files.Application.Dtos;

/// <summary>
/// 上传文件响应 DTO，用于返回文件编号、名称、分类和大小。
/// </summary>
public class UploadFileResDto
{
    /// <summary>
    /// 文件资源 ID。
    /// </summary>
    public Guid FileId { get; init; }

    /// <summary>
    /// 原始文件名。
    /// </summary>
    public string FileName { get; init; } = string.Empty;

    /// <summary>
    /// 文件分类。
    /// </summary>
    public string FileCategory { get; init; } = string.Empty;

    /// <summary>
    /// 文件大小，单位字节。
    /// </summary>
    public long FileSize { get; init; }
}
