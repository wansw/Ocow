using Ocow.Identity.Application.Dtos;
using Ocow.Identity.Application.Interfaces;
using Ocow.Identity.Domain.Models;

namespace Ocow.Identity.Application.Services;

/// <summary>
/// 角色权限应用服务，用于管理 RBAC 角色和权限点。
/// </summary>
public class RolePermissionAppService : IRolePermissionAppService
{
    private readonly IIdentityRepository _repository;

    /// <summary>
    /// 创建角色权限应用服务。
    /// </summary>
    public RolePermissionAppService(IIdentityRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// 查询角色列表。
    /// </summary>
    public async Task<IReadOnlyList<RoleResDto>> GetRolesAsync(CancellationToken cancellationToken = default)
    {
        var roles = await _repository.GetRolesAsync(cancellationToken);
        return roles.Select(MapRoleToResDto).ToList();
    }

    /// <summary>
    /// 保存角色。
    /// </summary>
    public async Task<RoleResDto> SaveRoleAsync(Guid? id, RoleReqDto reqDto, CancellationToken cancellationToken = default)
    {
        var role = new RoleModel
        {
            Id = id ?? Guid.NewGuid(),
            Code = reqDto.Code,
            Name = reqDto.Name
        };

        var result = await _repository.SaveRoleAsync(role, cancellationToken);
        return MapRoleToResDto(result);
    }

    /// <summary>
    /// 查询权限点列表。
    /// </summary>
    public async Task<IReadOnlyList<PermissionResDto>> GetPermissionsAsync(CancellationToken cancellationToken = default)
    {
        var permissions = await _repository.GetPermissionsAsync(cancellationToken);
        return permissions.Select(x => new PermissionResDto
        {
            Id = x.Id,
            Code = x.Code,
            Name = x.Name,
            Module = x.Module
        }).ToList();
    }

    /// <summary>
    /// 绑定角色权限点。
    /// </summary>
    public async Task BindRolePermissionsAsync(Guid roleId, BindRolePermissionsReqDto reqDto, CancellationToken cancellationToken = default)
    {
        await _repository.BindRolePermissionsAsync(roleId, reqDto.PermissionIds, cancellationToken);
    }

    /// <summary>
    /// 将角色实体转换为响应 DTO。
    /// </summary>
    private static RoleResDto MapRoleToResDto(RoleModel role)
    {
        return new RoleResDto
        {
            Id = role.Id,
            Code = role.Code,
            Name = role.Name
        };
    }
}
