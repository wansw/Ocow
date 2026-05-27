import { createRouter, createWebHistory } from 'vue-router';
import { getAuthSession } from '../utils/authStorage';

const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: '/login',
      name: 'login',
      component: () => import('../views/LoginView.vue'),
      meta: { anonymous: true }
    },
    {
      path: '/',
      component: () => import('../layout/AdminLayout.vue'),
      children: [
        {
          path: '',
          name: 'dashboard',
          component: () => import('../views/DashboardView.vue')
        },
        {
          path: 'identity/admin-users',
          name: 'admin-users',
          component: () => import('../views/AdminUsersView.vue')
        },
        {
          path: 'identity/roles',
          name: 'roles',
          component: () => import('../views/RolesView.vue')
        },
        {
          path: 'identity/permissions',
          name: 'permissions',
          component: () => import('../views/PermissionsView.vue')
        },
        {
          path: 'identity/menus',
          name: 'menus',
          component: () => import('../views/MenusView.vue')
        },
        {
          path: 'orders',
          name: 'orders',
          component: () => import('../views/OrdersView.vue')
        },
        {
          path: 'jobs',
          name: 'jobs',
          component: () => import('../views/JobsView.vue')
        }
      ]
    },
    {
      path: '/:pathMatch(.*)*',
      name: 'not-found',
      component: () => import('../views/NotFoundView.vue')
    }
  ]
});

router.beforeEach((to) => {
  const session = getAuthSession();
  if (!to.meta.anonymous && !session) {
    return { name: 'login', query: { redirect: to.fullPath } };
  }

  if (to.name === 'login' && session) {
    return { name: 'dashboard' };
  }

  return true;
});

export default router;
