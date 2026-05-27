<template>
  <div class="app-shell">
    <aside class="sidebar">
      <div class="sidebar-brand">
        <div class="brand-mark">O</div>
        <div>
          <p class="brand-title">Ocow Admin</p>
          <p class="brand-subtitle">后台管理</p>
        </div>
      </div>

      <nav class="sidebar-nav">
        <el-menu
          :default-active="route.path"
          background-color="#172033"
          text-color="#cbd5e4"
          active-text-color="#ffffff"
          router
        >
          <el-menu-item index="/">
            <el-icon><LayoutDashboard :size="18" /></el-icon>
            <span>工作台</span>
          </el-menu-item>

          <template v-for="item in navigationItems" :key="item.id">
            <el-sub-menu v-if="item.children.length" :index="item.path">
              <template #title>
                <el-icon><component :is="resolveIcon(item.icon)" :size="18" /></el-icon>
                <span>{{ item.title }}</span>
              </template>
              <el-menu-item v-for="child in item.children" :key="child.id" :index="child.path">
                <el-icon><component :is="resolveIcon(child.icon)" :size="18" /></el-icon>
                <span>{{ child.title }}</span>
              </el-menu-item>
            </el-sub-menu>
            <el-menu-item v-else :index="item.path">
              <el-icon><component :is="resolveIcon(item.icon)" :size="18" /></el-icon>
              <span>{{ item.title }}</span>
            </el-menu-item>
          </template>
        </el-menu>
      </nav>
    </aside>

    <section class="main-area">
      <header class="topbar">
        <div class="topbar-title">
          <h1>{{ currentTitle }}</h1>
          <p>{{ permissionCount }} 个权限点已加载</p>
        </div>
        <el-button :icon="LogOut" @click="handleLogout">退出</el-button>
      </header>

      <router-view />
    </section>
  </div>
</template>

<script setup lang="ts">
import {
  BriefcaseBusiness,
  KeyRound,
  LayoutDashboard,
  ListTree,
  LogOut,
  Menu,
  PackageCheck,
  Shield,
  Timer,
  UserCog,
  Users
} from 'lucide-vue-next';
import { ElMessage } from 'element-plus';
import { computed, onMounted, ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { logoutAdmin } from '../api/auth';
import { getProfileMenus } from '../api/identity';
import type { NavigationItem } from '../types/admin';
import { clearAuthSession, getAuthSession } from '../utils/authStorage';
import { buildNavigationItems } from '../utils/menu';
import { hasAnyPermission } from '../utils/permissions';

const route = useRoute();
const router = useRouter();
const navigationItems = ref<NavigationItem[]>([]);

const staticNavigation: NavigationItem[] = [
  {
    id: 'orders',
    title: '订单管理',
    path: '/orders',
    icon: 'package-check',
    children: []
  },
  {
    id: 'jobs',
    title: '任务管理',
    path: '/jobs',
    icon: 'timer',
    children: []
  }
];

const routeTitles: Record<string, string> = {
  '/': '工作台',
  '/identity/admin-users': '后台人员',
  '/identity/roles': '角色管理',
  '/identity/permissions': '权限点',
  '/identity/menus': '菜单管理',
  '/orders': '订单管理',
  '/jobs': '任务管理'
};

const permissionCount = computed(() => getAuthSession()?.permissions.length ?? 0);
const currentTitle = computed(() => routeTitles[route.path] ?? '后台管理');

/**
 * 解析菜单图标名称为 lucide 组件。
 */
function resolveIcon(icon?: string | null) {
  const icons = {
    briefcase: BriefcaseBusiness,
    'key-round': KeyRound,
    menu: Menu,
    shield: Shield,
    timer: Timer,
    'user-cog': UserCog,
    users: Users,
    'package-check': PackageCheck
  };
  return icons[(icon || '').toLowerCase() as keyof typeof icons] ?? ListTree;
}

/**
 * 加载当前管理员可见菜单并合并业务后台入口。
 */
async function loadNavigation(): Promise<void> {
  const dynamicMenus = await getProfileMenus().catch(() => []);
  const dynamicNavigation = buildNavigationItems(dynamicMenus);
  const businessNavigation = staticNavigation.filter((item) => {
    if (item.id === 'orders') {
      return hasAnyPermission(['order.read', 'order.ship']);
    }
    if (item.id === 'jobs') {
      return hasAnyPermission(['scheduler.trigger']);
    }
    return true;
  });

  navigationItems.value = [...dynamicNavigation, ...businessNavigation];
}

/**
 * 退出当前后台登录会话。
 */
async function handleLogout(): Promise<void> {
  const session = getAuthSession();
  if (session?.refreshToken) {
    await logoutAdmin(session.refreshToken).catch(() => false);
  }

  clearAuthSession();
  ElMessage.success('已退出登录');
  await router.replace({ name: 'login' });
}

onMounted(loadNavigation);
</script>
