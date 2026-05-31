using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Mvc;
using Ocow.AspNetCore.Controllers;
using Ocow.Auth.Attributes;
using Ocow.Files.Api.Dtos;
using Ocow.Files.Application.Dtos;
using Ocow.Files.Application.Interfaces;
using Ocow.Files.Application.Models;
using Ocow.Shared.Dtos;

namespace Ocow.Files.Api.Controllers.Admin;

/// <summary>
/// 后台文件接口，用于后台管理员上传业务文件。
/// </summary>
[Route("api/admin/files")]
[Tags("后台文件")]
public class AdminFilesController : AdminController
{
    private readonly IFileUploadAppService _fileUploadAppService;

    /// <summary>
    /// 创建后台文件 Controller。
    /// </summary>
    public AdminFilesController(IFileUploadAppService fileUploadAppService)
    {
        _fileUploadAppService = fileUploadAppService;
    }

    /// <summary>
    /// 上传后台业务文件，并保存文件元数据。
    /// </summary>
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    [PermissionAuthorize("file.upload")]
    public async Task<ApiResDto<UploadFileResDto>> UploadAsync([FromForm] UploadFileReqDto reqDto, CancellationToken cancellationToken)
    {
        await using var stream = reqDto.File.OpenReadStream();
        var result = await _fileUploadAppService.UploadAsync(new FileUploadCommand
        {
            OriginalName = reqDto.File.FileName,
            MimeType = reqDto.File.ContentType,
            Length = reqDto.File.Length,
            Content = stream,
            BizType = reqDto.BizType,
            BizId = reqDto.BizId,
            UploaderId = ResolveUserId(),
            UploaderScope = "admin"
        }, cancellationToken);

        return Success(result);
    }

    /// <summary>
    /// 从当前管理员声明中解析用户 ID。
    /// </summary>
    private Guid? ResolveUserId()
    {
        var value = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var userId) ? userId : null;
    }
}
