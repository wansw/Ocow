import type { AuthSession } from '../types/admin';

const AUTH_SESSION_KEY = 'ocow.admin.auth.session';

/**
 * 保存后台登录会话到本地存储。
 */
export function saveAuthSession(session: AuthSession): void {
  localStorage.setItem(AUTH_SESSION_KEY, JSON.stringify(session));
}

/**
 * 读取后台登录会话，解析失败时会清理脏数据。
 */
export function getAuthSession(): AuthSession | null {
  const raw = localStorage.getItem(AUTH_SESSION_KEY);
  if (!raw) {
    return null;
  }

  try {
    const session = JSON.parse(raw) as AuthSession;
    if (!session.accessToken || !session.refreshToken) {
      clearAuthSession();
      return null;
    }

    return {
      accessToken: session.accessToken,
      refreshToken: session.refreshToken,
      expiresAt: session.expiresAt,
      permissions: Array.isArray(session.permissions) ? session.permissions : []
    };
  } catch {
    clearAuthSession();
    return null;
  }
}

/**
 * 清理后台登录会话。
 */
export function clearAuthSession(): void {
  localStorage.removeItem(AUTH_SESSION_KEY);
}
