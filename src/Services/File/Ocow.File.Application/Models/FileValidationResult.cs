using Ocow.Files.Domain.Enums;

namespace Ocow.Files.Application.Models;

/// <summary>
/// 文件校验结果，用于返回校验是否通过以及识别出的文件类型。
/// </summary>
public class FileValidationResult
{
    /// <summary>
    /// 是否校验通过。
    /// </summary>
    public bool IsValid { get; init; }

    /// <summary>
    /// 错误编码。
    /// </summary>
    public string Code { get; init; } = string.Empty;

    /// <summary>
    /// 错误消息。
    /// </summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// 标准化文件扩展名。
    /// </summary>
    public string Extension { get; init; } = string.Empty;

    /// <summary>
    /// 文件分类。
    /// </summary>
    public FileCategoryEnum FileCategory { get; init; }

    /// <summary>
    /// 创建成功校验结果。
    /// </summary>
    public static FileValidationResult Success(string extension, FileCategoryEnum fileCategory)
    {
        return new FileValidationResult
        {
            IsValid = true,
            Extension = extension,
            FileCategory = fileCategory
        };
    }

    /// <summary>
    /// 创建失败校验结果。
    /// </summary>
    public static FileValidationResult Fail(string code, string message, string extension = "")
    {
        return new FileValidationResult
        {
            IsValid = false,
            Code = code,
            Message = message,
            Extension = extension
        };
    }
}
