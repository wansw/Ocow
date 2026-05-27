import { getAuthSession } from './authStorage';

/**
 * 判断当前管理员是否拥有指定权限点。
 */
export function hasPermission(permissionCode?: string): boolean {
  if (!permissionCode) {
    return true;
  }

  const session = getAuthSession();
  return Boolean(session?.permissions.includes(permissionCode));
}

/**
 * 判断当前管理员是否拥有任意一个权限点。
 */
export function hasAnyPermission(permissionCodes: string[]): boolean {
  if (permissionCodes.length === 0) {
    return true;
  }

  const session = getAuthSession();
  return permissionCodes.some((code) => session?.permissions.includes(code));
}
