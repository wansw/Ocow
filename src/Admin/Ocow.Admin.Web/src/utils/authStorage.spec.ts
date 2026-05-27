import { describe, expect, it } from 'vitest';
import { clearAuthSession, getAuthSession, saveAuthSession } from './authStorage';

describe('authStorage', () => {
  it('saves and clears admin token session', () => {
    saveAuthSession({
      accessToken: 'access-token',
      refreshToken: 'refresh-token',
      expiresAt: '2026-05-27T12:00:00Z',
      permissions: ['identity.admin-user.read']
    });

    expect(getAuthSession()).toEqual({
      accessToken: 'access-token',
      refreshToken: 'refresh-token',
      expiresAt: '2026-05-27T12:00:00Z',
      permissions: ['identity.admin-user.read']
    });

    clearAuthSession();

    expect(getAuthSession()).toBeNull();
  });
});
