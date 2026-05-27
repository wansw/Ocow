<template>
  <main class="page">
    <div class="toolbar">
      <h2 class="toolbar-title">权限点</h2>
      <div class="toolbar-actions">
        <el-select v-model="moduleFilter" clearable placeholder="模块" style="width: 180px">
          <el-option v-for="module in modules" :key="module" :label="module" :value="module" />
        </el-select>
        <el-button :icon="RefreshCw" @click="loadPermissions">刷新</el-button>
      </div>
    </div>

    <section class="content-panel">
      <el-table v-loading="loading" :data="filteredPermissions" row-key="id">
        <el-table-column prop="module" label="模块" width="140" />
        <el-table-column prop="code" label="权限编码" min-width="220" />
        <el-table-column prop="name" label="权限名称" min-width="180" />
        <el-table-column label="编号" min-width="260">
          <template #default="{ row }">
            <span class="muted">{{ row.id }}</span>
          </template>
        </el-table-column>
      </el-table>
    </section>
  </main>
</template>

<script setup lang="ts">
import { RefreshCw } from 'lucide-vue-next';
import { computed, onMounted, ref } from 'vue';
import { getPermissions } from '../api/identity';
import type { Permission } from '../types/admin';

const loading = ref(false);
const moduleFilter = ref('');
const permissions = ref<Permission[]>([]);
const modules = computed(() => Array.from(new Set(permissions.value.map((item) => item.module))).sort());
const filteredPermissions = computed(() => {
  if (!moduleFilter.value) {
    return permissions.value;
  }

  return permissions.value.filter((item) => item.module === moduleFilter.value);
});

/**
 * 查询权限点列表。
 */
async function loadPermissions(): Promise<void> {
  loading.value = true;
  try {
    permissions.value = await getPermissions();
  } finally {
    loading.value = false;
  }
}

onMounted(loadPermissions);
</script>
