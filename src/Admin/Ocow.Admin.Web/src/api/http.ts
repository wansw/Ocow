import type { ApiResDto } from '../types/api';
import type { AuthTokenResDto } from '../types/admin';
import { clearAuthSession, getAuthSession, saveAuthSession } from '../utils/authStorage';

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL || '';

/**
 * 后台接口业务异常。
 */
export class ApiBusinessError extends Error {
  code: string;
  traceId?: string;

  /**
   * 创建后台接口业务异常。
   */
  constructor(code: string, message: string, traceId?: string) {
    super(message);
    this.name = 'ApiBusinessError';
    this.code = code;
    this.traceId = traceId;
  }
}

/**
 * 后台接口 HTTP 异常。
 */
export class ApiHttpError extends Error {
  status: number;

  /**
   * 创建后台接口 HTTP 异常。
   */
  constructor(status: number, message: string) {
    super(message);
    this.name = 'ApiHttpError';
    this.status = status;
  }
}

/**
 * 发送后台接口请求，自动注入 Admin JWT 并解包 ApiResDto。
 */
export async function adminRequest<T>(path: string, options: RequestInit = {}): Promise<T> {
  return requestWithAuth<T>(path, options, true);
}

/**
 * 发送不带登录态要求的后台接口请求。
 */
export async function anonymousRequest<T>(path: string, options: RequestInit = {}): Promise<T> {
  return requestWithAuth<T>(path, options, false);
}

/**
 * 拼接接口地址。
 */
export function resolveApiUrl(path: string): string {
  if (/^https?:\/\//i.test(path)) {
    return path;
  }

  return `${apiBaseUrl}${path}`;
}

async function requestWithAuth<T>(path: string, options: RequestInit, useAuth: boolean): Promise<T> {
  const response = await sendRequest(path, options, useAuth);
  if (response.status === 401 && useAuth && !path.includes('/refresh-token')) {
    const refreshed = await refreshToken();
    if (refreshed) {
      const retryResponse = await sendRequest(path, options, useAuth);
      return unwrapResponse<T>(retryResponse);
    }

    clearAuthSession();
  }

  return unwrapResponse<T>(response);
}

async function sendRequest(path: string, options: RequestInit, useAuth: boolean): Promise<Response> {
  const headers = new Headers(options.headers);
  const session = getAuthSession();

  if (useAuth && session?.accessToken && !headers.has('Authorization')) {
    headers.set('Authorization', `Bearer ${session.accessToken}`);
  }

  if (options.body && !(options.body instanceof FormData) && !headers.has('Content-Type')) {
    headers.set('Content-Type', 'application/json');
  }

  return fetch(resolveApiUrl(path), {
    ...options,
    headers
  });
}

async function unwrapResponse<T>(response: Response): Promise<T> {
  if (!response.ok) {
    throw new ApiHttpError(response.status, `请求失败：${response.status}`);
  }

  const body = await response.json() as ApiResDto<T>;
  if (!body.success) {
    throw new ApiBusinessError(body.code, body.message, body.traceId);
  }

  return body.data as T;
}

async function refreshToken(): Promise<boolean> {
  const session = getAuthSession();
  if (!session?.refreshToken) {
    return false;
  }

  try {
    const token = await anonymousRequest<AuthTokenResDto>('/api/admin/auth/refresh-token', {
      method: 'POST',
      body: JSON.stringify({ refreshToken: session.refreshToken })
    });

    saveAuthSession({
      accessToken: token.accessToken,
      refreshToken: token.refreshToken,
      expiresAt: token.expiresAt,
      permissions: token.permissions
    });
    return true;
  } catch {
    return false;
  }
}
