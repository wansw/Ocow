import type { PageReqDto, PageResDto } from '../types/api';
import type {
  AdminMenu,
  AdminUser,
  AdminUserReqDto,
  BindRolePermissionsReqDto,
  MenuReqDto,
  Permission,
  Role,
  RoleReqDto
} from '../types/admin';
import { adminRequest } from './http';

/**
 * 查询后台管理员分页列表。
 */
export function getAdminUsers(reqDto: PageReqDto): Promise<PageResDto<AdminUser>> {
  const query = new URLSearchParams({
    pageIndex: String(reqDto.pageIndex),
    pageSize: String(reqDto.pageSize)
  });
  return adminRequest<PageResDto<AdminUser>>(`/api/admin/users?${query.toString()}`);
}

/**
 * 创建后台管理员账号。
 */
export function createAdminUser(reqDto: AdminUserReqDto): Promise<AdminUser> {
  return adminRequest<AdminUser>('/api/admin/users', {
    method: 'POST',
    body: JSON.stringify(reqDto)
  });
}

/**
 * 禁用后台管理员账号。
 */
export function disableAdminUser(id: string): Promise<boolean> {
  return adminRequest<boolean>(`/api/admin/users/${id}/disable`, {
    method: 'POST'
  });
}

/**
 * 查询角色列表。
 */
export function getRoles(): Promise<Role[]> {
  return adminRequest<Role[]>('/api/admin/roles');
}

/**
 * 创建角色。
 */
export function createRole(reqDto: RoleReqDto): Promise<Role> {
  return adminRequest<Role>('/api/admin/roles', {
    method: 'POST',
    body: JSON.stringify(reqDto)
  });
}

/**
 * 修改角色。
 */
export function updateRole(id: string, reqDto: RoleReqDto): Promise<Role> {
  return adminRequest<Role>(`/api/admin/roles/${id}`, {
    method: 'PUT',
    body: JSON.stringify(reqDto)
  });
}

/**
 * 绑定角色权限点。
 */
export function bindRolePermissions(id: string, reqDto: BindRolePermissionsReqDto): Promise<boolean> {
  return adminRequest<boolean>(`/api/admin/roles/${id}/permissions`, {
    method: 'PUT',
    body: JSON.stringify(reqDto)
  });
}

/**
 * 查询权限点列表。
 */
export function getPermissions(): Promise<Permission[]> {
  return adminRequest<Permission[]>('/api/admin/permissions');
}

/**
 * 查询后台菜单树。
 */
export function getMenus(): Promise<AdminMenu[]> {
  return adminRequest<AdminMenu[]>('/api/admin/menus');
}

/**
 * 查询当前管理员可见菜单树。
 */
export function getProfileMenus(): Promise<AdminMenu[]> {
  return adminRequest<AdminMenu[]>('/api/admin/profile/menus');
}

/**
 * 创建后台菜单。
 */
export function createMenu(reqDto: MenuReqDto): Promise<AdminMenu> {
  return adminRequest<AdminMenu>('/api/admin/menus', {
    method: 'POST',
    body: JSON.stringify(reqDto)
  });
}

/**
 * 修改后台菜单。
 */
export function updateMenu(id: string, reqDto: MenuReqDto): Promise<AdminMenu> {
  return adminRequest<AdminMenu>(`/api/admin/menus/${id}`, {
    method: 'PUT',
    body: JSON.stringify(reqDto)
  });
}
