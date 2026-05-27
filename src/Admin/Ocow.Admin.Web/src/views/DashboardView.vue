<template>
  <main class="page">
    <div class="summary-grid">
      <section class="summary-card">
        <span>后台入口</span>
        <strong>{{ navCount }}</strong>
      </section>
      <section class="summary-card">
        <span>权限点</span>
        <strong>{{ permissionCount }}</strong>
      </section>
      <section class="summary-card">
        <span>登录范围</span>
        <strong>Admin</strong>
      </section>
      <section class="summary-card">
        <span>接口入口</span>
        <strong>/api</strong>
      </section>
    </div>

    <section class="content-panel" style="margin-top: 16px">
      <el-descriptions :column="2" border>
        <el-descriptions-item label="认证模式">Admin JWT</el-descriptions-item>
        <el-descriptions-item label="部署形态">nginx + Gateway</el-descriptions-item>
        <el-descriptions-item label="Token 过期时间">{{ session?.expiresAt || '-' }}</el-descriptions-item>
        <el-descriptions-item label="权限点数量">{{ permissionCount }}</el-descriptions-item>
      </el-descriptions>
    </section>
  </main>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import { getAuthSession } from '../utils/authStorage';

const session = computed(() => getAuthSession());
const permissionCount = computed(() => session.value?.permissions.length ?? 0);
const navCount = computed(() => {
  const permissions = session.value?.permissions ?? [];
  return permissions.filter((code) => code.endsWith('.read') || code === 'order.read').length;
});
</script>
