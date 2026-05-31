using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Mvc;
using Ocow.AspNetCore.Controllers;
using Ocow.Files.Api.Dtos;
using Ocow.Files.Application.Dtos;
using Ocow.Files.Application.Interfaces;
using Ocow.Files.Application.Models;
using Ocow.Shared.Dtos;

namespace Ocow.Files.Api.Controllers.Client;

/// <summary>
/// 用户端文件接口，用于上传用户业务文件。
/// </summary>
[Route("api/files")]
[Tags("用户端文件")]
public class FilesController : ClientController
{
    private readonly IFileUploadAppService _fileUploadAppService;

    /// <summary>
    /// 创建用户端文件 Controller。
    /// </summary>
    public FilesController(IFileUploadAppService fileUploadAppService)
    {
        _fileUploadAppService = fileUploadAppService;
    }

    /// <summary>
    /// 上传用户端文件，并保存文件元数据。
    /// </summary>
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
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
            UploaderScope = "client"
        }, cancellationToken);

        return Success(result);
    }

    /// <summary>
    /// 从当前用户声明中解析用户 ID。
    /// </summary>
    private Guid? ResolveUserId()
    {
        var value = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var userId) ? userId : null;
    }
}
