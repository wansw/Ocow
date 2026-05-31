namespace Ocow.Files.Application.Options;

/// <summary>
/// 文件上传配置，用于控制允许的扩展名和不同分类的大小限制。
/// </summary>
public class FileUploadOption
{
    /// <summary>
    /// 配置节点名称。
    /// </summary>
    public const string SectionName = "FileUpload";

    /// <summary>
    /// 文本文件最大字节数。
    /// </summary>
    public long MaxTxtBytes { get; set; } = 2 * 1024 * 1024;

    /// <summary>
    /// Excel 文件最大字节数。
    /// </summary>
    public long MaxExcelBytes { get; set; } = 20 * 1024 * 1024;

    /// <summary>
    /// 图片文件最大字节数。
    /// </summary>
    public long MaxImageBytes { get; set; } = 10 * 1024 * 1024;

    /// <summary>
    /// 允许上传的文件扩展名。
    /// </summary>
    public string[] AllowedExtensions { get; set; } =
    [
        ".txt",
        ".xls",
        ".xlsx",
        ".jpg",
        ".jpeg",
        ".png",
        ".webp"
    ];
}
