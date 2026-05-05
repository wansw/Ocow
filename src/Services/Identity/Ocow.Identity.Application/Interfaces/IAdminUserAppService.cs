using Ocow.Identity.Application.Dtos;
using Ocow.Shared.Dtos;

namespace Ocow.Identity.Application.Interfaces;

/// <summary>
/// 管理员应用服务接口，用于后台账号管理。/// </summary>
public interface IAdminUserAppService
{
    /// <summary>
    /// 分页查询管理员列表。    /// </summary>
    Task<PageResDto<AdminUserResDto>> GetListAsync(PageReqDto reqDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// 创建管理员账号。    /// </summary>
    Task<AdminUserResDto> CreateAsync(AdminUserReqDto reqDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// 禁用管理员账号。    /// </summary>
    Task DisableAsync(Guid id, CancellationToken cancellationToken = default);
}
