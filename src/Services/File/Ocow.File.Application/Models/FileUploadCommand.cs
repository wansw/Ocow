namespace Ocow.Files.Application.Models;

/// <summary>
/// 文件上传命令，用于应用层接收 Controller 转换后的上传参数。
/// </summary>
public class FileUploadCommand
{
    /// <summary>
    /// 原始文件名。
    /// </summary>
    public string OriginalName { get; init; } = string.Empty;

    /// <summary>
    /// 浏览器上传的 MIME Type。
    /// </summary>
    public string? MimeType { get; init; }

    /// <summary>
    /// 文件大小，单位字节。
    /// </summary>
    public long Length { get; init; }

    /// <summary>
    /// 文件内容流。
    /// </summary>
    public Stream Content { get; init; } = Stream.Null;

    /// <summary>
    /// 业务类型。
    /// </summary>
    public string? BizType { get; init; }

    /// <summary>
    /// 业务 ID。
    /// </summary>
    public string? BizId { get; init; }

    /// <summary>
    /// 上传人 ID。
    /// </summary>
    public Guid? UploaderId { get; init; }

    /// <summary>
    /// 上传人身份范围。
    /// </summary>
    public string? UploaderScope { get; init; }
}
