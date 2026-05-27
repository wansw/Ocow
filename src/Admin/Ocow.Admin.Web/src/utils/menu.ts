import type { AdminMenu, NavigationItem, SelectOption } from '../types/admin';

/**
 * 将后端菜单树转换为前端可见导航树。
 */
export function buildNavigationItems(menus: AdminMenu[]): NavigationItem[] {
  return menus
    .filter((menu) => menu.type === 1 && menu.isEnabled && menu.isVisible)
    .sort((a, b) => a.sort - b.sort || a.code.localeCompare(b.code))
    .map((menu) => ({
      id: menu.id,
      title: menu.name,
      path: menu.path || '/',
      icon: menu.icon,
      children: buildNavigationItems(menu.children ?? [])
    }));
}

/**
 * 将菜单树展开为下拉选择项。
 */
export function flattenMenuOptions(menus: AdminMenu[], parentLabel = ''): SelectOption[] {
  return menus
    .slice()
    .sort((a, b) => a.sort - b.sort || a.code.localeCompare(b.code))
    .flatMap((menu) => {
      const label = parentLabel ? `${parentLabel} / ${menu.name}` : menu.name;
      return [
        { label, value: menu.id },
        ...flattenMenuOptions(menu.children ?? [], label)
      ];
    });
}

/**
 * 将菜单树展开为普通数组。
 */
export function flattenMenus(menus: AdminMenu[]): AdminMenu[] {
  return menus.flatMap((menu) => [menu, ...flattenMenus(menu.children ?? [])]);
}
