namespace Ocow.Files.Application.Models;

/// <summary>
/// 文件存储保存上下文，用于传递待保存文件的扩展名和内容流。
/// </summary>
public class FileStorageSaveContext
{
    /// <summary>
    /// 标准化文件扩展名。
    /// </summary>
    public string Extension { get; init; } = string.Empty;

    /// <summary>
    /// 待保存文件内容流。
    /// </summary>
    public Stream Content { get; init; } = Stream.Null;
}
