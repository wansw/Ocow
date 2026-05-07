using Ocow.Identity.Application.Dtos;
using Ocow.Identity.Application.Interfaces;
using Ocow.Identity.Domain.Models;
using Ocow.Shared.Dtos;

namespace Ocow.Identity.Application.Services;

/// <summary>
/// 管理员应用服务，用于后台账号管理。/// </summary>
public class AdminUserAppService : IAdminUserAppService
{
    private readonly IIdentityRepository _repository;

    /// <summary>
    /// 创建管理员应用服务。    
    /// </summary>
    public AdminUserAppService(IIdentityRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// 分页查询管理员列表。    
    /// </summary>
    public async Task<PageResDto<AdminUserResDto>> GetListAsync(PageReqDto reqDto, CancellationToken cancellationToken = default)
    {
        var result = await _repository.GetAdminUsersAsync(reqDto, cancellationToken);
        return new PageResDto<AdminUserResDto>
        {
            Items = result.Items.Select(MapToResDto).ToList(),
            Total = result.Total,
            PageIndex = result.PageIndex,
            PageSize = result.PageSize
        };
    }

    /// <summary>
    /// 创建管理员账号。    
    /// </summary>
    public async Task<AdminUserResDto> CreateAsync(AdminUserReqDto reqDto, CancellationToken cancellationToken = default)
    {
        var adminUser = new AdminUser
        {
            Id = Guid.NewGuid(),
            UserName = reqDto.UserName,
            DisplayName = reqDto.DisplayName,
            PasswordHash = PasswordHashService.Hash(reqDto.Password)
        };

        await _repository.AddAdminUserAsync(adminUser, reqDto.RoleIds, cancellationToken);
        return MapToResDto(adminUser);
    }

    /// <summary>
    /// 禁用管理员账号。    
    /// </summary>
    public async Task DisableAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _repository.DisableAdminUserAsync(id, cancellationToken);
    }

    /// <summary>
    /// 将管理员实体转换为响。DTO。    
    /// </summary>
    private static AdminUserResDto MapToResDto(AdminUser adminUser)
    {
        return new AdminUserResDto
        {
            Id = adminUser.Id,
            UserName = adminUser.UserName,
            DisplayName = adminUser.DisplayName,
            Status = adminUser.Status
        };
    }
}
