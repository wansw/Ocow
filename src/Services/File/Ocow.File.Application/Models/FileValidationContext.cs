namespace Ocow.Files.Application.Models;

/// <summary>
/// 文件校验上下文，用于传递上传文件的名称、大小、MIME 和内容流。
/// </summary>
public class FileValidationContext
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
}
