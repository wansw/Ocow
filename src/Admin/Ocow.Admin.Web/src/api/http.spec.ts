import { describe, expect, it, vi } from 'vitest';
import { adminRequest } from './http';
import { saveAuthSession } from '../utils/authStorage';

describe('adminRequest', () => {
  it('adds admin bearer token and unwraps ApiResDto data', async () => {
    saveAuthSession({
      accessToken: 'access-token',
      refreshToken: 'refresh-token',
      expiresAt: '2026-05-27T12:00:00Z',
      permissions: ['identity.role.read']
    });
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      status: 200,
      json: async () => ({
        success: true,
        code: '0',
        message: 'success',
        data: { id: 'role-1' }
      })
    });
    vi.stubGlobal('fetch', fetchMock);

    const result = await adminRequest<{ id: string }>('/api/admin/roles');

    expect(result).toEqual({ id: 'role-1' });
    const [, init] = fetchMock.mock.calls[0];
    expect((init.headers as Headers).get('Authorization')).toBe('Bearer access-token');
  });

  it('throws ApiBusinessError when ApiResDto reports failure', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({
      ok: true,
      status: 200,
      json: async () => ({
        success: false,
        code: 'IDENTITY_ERROR',
        message: '权限不足',
        traceId: 'trace-1'
      })
    }));

    await expect(adminRequest('/api/admin/roles')).rejects.toMatchObject({
      code: 'IDENTITY_ERROR',
      message: '权限不足',
      traceId: 'trace-1'
    });
  });
});
