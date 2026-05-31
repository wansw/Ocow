using Microsoft.Extensions.Options;
using Ocow.Files.Application.Interfaces;
using Ocow.Files.Application.Models;
using Ocow.Files.Application.Options;
using Ocow.Files.Domain.Enums;

namespace Ocow.Files.Infrastructure.Validation;

/// <summary>
/// 文件校验器，用于校验上传文件大小、扩展名和真实文件头。
/// </summary>
public class FileValidator : IFileValidator
{
    private static readonly HashSet<string> DangerousExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".exe",
        ".sh",
        ".bat",
        ".js",
        ".html",
        ".php",
        ".jsp",
        ".jar",
        ".class"
    };

    private readonly FileUploadOption _option;

    /// <summary>
    /// 创建文件校验器。
    /// </summary>
    public FileValidator(IOptions<FileUploadOption> option)
    {
        _option = option.Value;
    }

    /// <summary>
    /// 校验上传文件是否允许保存。
    /// </summary>
    public async Task<FileValidationResult> ValidateAsync(FileValidationContext context, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(context.OriginalName))
        {
            return FileValidationResult.Fail("FILE_NAME_REQUIRED", "文件名不能为空。");
        }

        if (context.Length <= 0)
        {
            return FileValidationResult.Fail("FILE_EMPTY", "文件不能为空。");
        }

        var extension = Path.GetExtension(context.OriginalName).ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(extension))
        {
            return FileValidationResult.Fail("FILE_EXTENSION_REQUIRED", "文件扩展名不能为空。");
        }

        if (DangerousExtensions.Contains(extension) || !_option.AllowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            return FileValidationResult.Fail("FILE_EXTENSION_NOT_ALLOWED", "不允许上传该类型文件。", extension);
        }

        var fileCategory = ResolveFileCategory(extension);
        var maxBytes = ResolveMaxBytes(fileCategory);
        if (context.Length > maxBytes)
        {
            return FileValidationResult.Fail("FILE_SIZE_EXCEEDED", "文件大小超过限制。", extension);
        }

        if (RequiresMagicNumberCheck(fileCategory) && !context.Content.CanSeek)
        {
            return FileValidationResult.Fail("FILE_STREAM_NOT_SEEKABLE", "文件流不支持格式校验。", extension);
        }

        if (RequiresMagicNumberCheck(fileCategory))
        {
            var matched = await MatchMagicNumberAsync(context.Content, extension, cancellationToken);
            if (!matched)
            {
                return FileValidationResult.Fail("FILE_FORMAT_INVALID", "文件真实格式与扩展名不匹配。", extension);
            }
        }

        return FileValidationResult.Success(extension, fileCategory);
    }

    /// <summary>
    /// 根据扩展名解析文件分类。
    /// </summary>
    private static FileCategoryEnum ResolveFileCategory(string extension)
    {
        return extension switch
        {
            ".txt" => FileCategoryEnum.Txt,
            ".xls" or ".xlsx" => FileCategoryEnum.Excel,
            ".jpg" or ".jpeg" or ".png" or ".webp" => FileCategoryEnum.Image,
            _ => throw new InvalidOperationException($"不支持的文件扩展名：{extension}")
        };
    }

    /// <summary>
    /// 根据文件分类获取最大字节数。
    /// </summary>
    private long ResolveMaxBytes(FileCategoryEnum fileCategory)
    {
        return fileCategory switch
        {
            FileCategoryEnum.Txt => _option.MaxTxtBytes,
            FileCategoryEnum.Excel => _option.MaxExcelBytes,
            FileCategoryEnum.Image => _option.MaxImageBytes,
            _ => _option.MaxTxtBytes
        };
    }

    /// <summary>
    /// 判断文件分类是否需要校验真实文件头。
    /// </summary>
    private static bool RequiresMagicNumberCheck(FileCategoryEnum fileCategory)
    {
        return fileCategory is FileCategoryEnum.Excel or FileCategoryEnum.Image;
    }

    /// <summary>
    /// 校验文件头是否匹配扩展名。
    /// </summary>
    private static async Task<bool> MatchMagicNumberAsync(Stream content, string extension, CancellationToken cancellationToken)
    {
        var originalPosition = content.Position;
        try
        {
            content.Position = 0;
            var buffer = new byte[12];
            var read = await content.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken);

            return extension switch
            {
                ".xlsx" => read >= 4 && buffer[0] == 0x50 && buffer[1] == 0x4B && buffer[2] == 0x03 && buffer[3] == 0x04,
                ".xls" => read >= 8 && buffer[0] == 0xD0 && buffer[1] == 0xCF && buffer[2] == 0x11 && buffer[3] == 0xE0,
                ".jpg" or ".jpeg" => read >= 3 && buffer[0] == 0xFF && buffer[1] == 0xD8 && buffer[2] == 0xFF,
                ".png" => read >= 8 && buffer[0] == 0x89 && buffer[1] == 0x50 && buffer[2] == 0x4E && buffer[3] == 0x47,
                ".webp" => read >= 12 && buffer[0] == 0x52 && buffer[1] == 0x49 && buffer[2] == 0x46 && buffer[3] == 0x46 &&
                           buffer[8] == 0x57 && buffer[9] == 0x45 && buffer[10] == 0x42 && buffer[11] == 0x50,
                _ => true
            };
        }
        finally
        {
            content.Position = originalPosition;
        }
    }
}
