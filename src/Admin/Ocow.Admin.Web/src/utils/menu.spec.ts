import { describe, expect, it } from 'vitest';
import { buildNavigationItems, flattenMenuOptions } from './menu';
import type { AdminMenu } from '../types/admin';

describe('menu utilities', () => {
  const menus: AdminMenu[] = [
    {
      id: 'root',
      parentId: null,
      code: 'identity',
      name: '权限中心',
      type: 1,
      path: '/identity',
      component: 'Layout',
      icon: 'shield',
      sort: 10,
      permissionId: null,
      permissionCode: null,
      isVisible: true,
      isEnabled: true,
      children: [
        {
          id: 'roles',
          parentId: 'root',
          code: 'identity.roles',
          name: '角色管理',
          type: 1,
          path: '/identity/roles',
          component: 'identity/roles/index',
          icon: 'users',
          sort: 10,
          permissionId: 'permission-1',
          permissionCode: 'identity.role.read',
          isVisible: true,
          isEnabled: true,
          children: []
        },
        {
          id: 'hidden',
          parentId: 'root',
          code: 'identity.hidden',
          name: '隐藏菜单',
          type: 1,
          path: '/identity/hidden',
          component: 'identity/hidden/index',
          icon: null,
          sort: 20,
          permissionId: null,
          permissionCode: null,
          isVisible: false,
          isEnabled: true,
          children: []
        }
      ]
    }
  ];

  it('builds visible page navigation items recursively', () => {
    expect(buildNavigationItems(menus)).toEqual([
      {
        id: 'root',
        title: '权限中心',
        path: '/identity',
        icon: 'shield',
        children: [
          {
            id: 'roles',
            title: '角色管理',
            path: '/identity/roles',
            icon: 'users',
            children: []
          }
        ]
      }
    ]);
  });

  it('flattens menu tree into sorted select options', () => {
    expect(flattenMenuOptions(menus)).toEqual([
      { label: '权限中心', value: 'root' },
      { label: '权限中心 / 角色管理', value: 'roles' },
      { label: '权限中心 / 隐藏菜单', value: 'hidden' }
    ]);
  });
});
