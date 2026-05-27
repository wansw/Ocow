import type { AdminLoginReqDto, AuthTokenResDto } from '../types/admin';
import { anonymousRequest, adminRequest } from './http';

/**
 * 调用后台登录接口。
 */
export function loginAdmin(reqDto: AdminLoginReqDto): Promise<AuthTokenResDto> {
  return anonymousRequest<AuthTokenResDto>('/api/admin/auth/login', {
    method: 'POST',
    body: JSON.stringify(reqDto)
  });
}

/**
 * 调用后台退出登录接口。
 */
export function logoutAdmin(refreshToken: string): Promise<boolean> {
  return adminRequest<boolean>('/api/admin/auth/logout', {
    method: 'POST',
    body: JSON.stringify({ refreshToken })
  });
}
