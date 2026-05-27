<template>
  <main class="page">
    <div class="toolbar">
      <h2 class="toolbar-title">菜单管理</h2>
      <div class="toolbar-actions">
        <el-button :icon="RefreshCw" @click="loadData">刷新</el-button>
        <el-button v-if="canSave" :icon="Plus" type="primary" @click="openMenuDialog()">新增</el-button>
      </div>
    </div>

    <section class="content-panel">
      <el-table
        v-loading="loading"
        :data="menus"
        row-key="id"
        default-expand-all
        :tree-props="{ children: 'children' }"
      >
        <el-table-column prop="name" label="菜单名称" min-width="180" />
        <el-table-column prop="code" label="菜单编码" min-width="220" />
        <el-table-column label="类型" width="100">
          <template #default="{ row }">
            <el-tag :type="row.type === 1 ? 'primary' : 'warning'">{{ row.type === 1 ? '页面' : '按钮' }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="path" label="路由" min-width="170" />
        <el-table-column prop="permissionCode" label="权限点" min-width="190" />
        <el-table-column prop="sort" label="排序" width="90" />
        <el-table-column label="状态" width="130">
          <template #default="{ row }">
            <el-space>
              <el-tag :type="row.isVisible ? 'success' : 'info'">{{ row.isVisible ? '显示' : '隐藏' }}</el-tag>
              <el-tag :type="row.isEnabled ? 'success' : 'danger'">{{ row.isEnabled ? '启用' : '停用' }}</el-tag>
            </el-space>
          </template>
        </el-table-column>
        <el-table-column label="操作" width="110" fixed="right">
          <template #default="{ row }">
            <el-button v-if="canSave" :icon="Pencil" size="small" @click="openMenuDialog(row)">编辑</el-button>
          </template>
        </el-table-column>
      </el-table>
    </section>

    <el-dialog v-model="menuDialogVisible" :title="editingMenuId ? '编辑菜单' : '新增菜单'" width="720px">
      <el-form :model="menuForm" label-position="top">
        <div class="form-grid">
          <el-form-item label="父级菜单">
            <el-select v-model="menuForm.parentId" clearable filterable style="width: 100%">
              <el-option v-for="option in parentOptions" :key="option.value" :label="option.label" :value="option.value" />
            </el-select>
          </el-form-item>
          <el-form-item label="菜单类型">
            <el-segmented v-model="menuForm.type" :options="menuTypeOptions" />
          </el-form-item>
          <el-form-item label="菜单编码">
            <el-input v-model="menuForm.code" />
          </el-form-item>
          <el-form-item label="菜单名称">
            <el-input v-model="menuForm.name" />
          </el-form-item>
          <el-form-item label="路由路径">
            <el-input v-model="menuForm.path" />
          </el-form-item>
          <el-form-item label="组件路径">
            <el-input v-model="menuForm.component" />
          </el-form-item>
          <el-form-item label="图标">
            <el-input v-model="menuForm.icon" />
          </el-form-item>
          <el-form-item label="排序">
            <el-input-number v-model="menuForm.sort" :min="0" style="width: 100%" />
          </el-form-item>
          <el-form-item label="权限点" class="full">
            <el-select v-model="menuForm.permissionId" clearable filterable style="width: 100%">
              <el-option
                v-for="permission in permissions"
                :key="permission.id"
                :label="`${permission.name}（${permission.code}）`"
                :value="permission.id"
              />
            </el-select>
          </el-form-item>
          <el-form-item label="显示">
            <el-switch v-model="menuForm.isVisible" />
          </el-form-item>
          <el-form-item label="启用">
            <el-switch v-model="menuForm.isEnabled" />
          </el-form-item>
        </div>
      </el-form>
      <template #footer>
        <el-button @click="menuDialogVisible = false">取消</el-button>
        <el-button :loading="saving" type="primary" @click="submitMenu">保存</el-button>
      </template>
    </el-dialog>
  </main>
</template>

<script setup lang="ts">
import { Pencil, Plus, RefreshCw } from 'lucide-vue-next';
import { ElMessage } from 'element-plus';
import { computed, onMounted, reactive, ref } from 'vue';
import { createMenu, getMenus, getPermissions, updateMenu } from '../api/identity';
import type { AdminMenu, MenuReqDto, Permission } from '../types/admin';
import { flattenMenuOptions } from '../utils/menu';
import { hasPermission } from '../utils/permissions';

const loading = ref(false);
const saving = ref(false);
const menus = ref<AdminMenu[]>([]);
const permissions = ref<Permission[]>([]);
const editingMenuId = ref<string | null>(null);
const menuDialogVisible = ref(false);
const menuTypeOptions = [
  { label: '页面', value: 1 },
  { label: '按钮', value: 2 }
];
const menuForm = reactive<MenuReqDto>({
  parentId: null,
  code: '',
  name: '',
  type: 1,
  path: '',
  component: '',
  icon: '',
  sort: 0,
  permissionId: null,
  isVisible: true,
  isEnabled: true
});

const canSave = computed(() => hasPermission('identity.menu.save'));
const parentOptions = computed(() => flattenMenuOptions(menus.value).filter((item) => item.value !== editingMenuId.value));

/**
 * 查询菜单树和权限点。
 */
async function loadData(): Promise<void> {
  loading.value = true;
  try {
    const [menuResult, permissionResult] = await Promise.all([getMenus(), getPermissions()]);
    menus.value = menuResult;
    permissions.value = permissionResult;
  } finally {
    loading.value = false;
  }
}

/**
 * 打开菜单新增或编辑弹窗。
 */
function openMenuDialog(menu?: AdminMenu): void {
  editingMenuId.value = menu?.id ?? null;
  menuForm.parentId = menu?.parentId ?? null;
  menuForm.code = menu?.code ?? '';
  menuForm.name = menu?.name ?? '';
  menuForm.type = menu?.type ?? 1;
  menuForm.path = menu?.path ?? '';
  menuForm.component = menu?.component ?? '';
  menuForm.icon = menu?.icon ?? '';
  menuForm.sort = menu?.sort ?? 0;
  menuForm.permissionId = menu?.permissionId ?? null;
  menuForm.isVisible = menu?.isVisible ?? true;
  menuForm.isEnabled = menu?.isEnabled ?? true;
  menuDialogVisible.value = true;
}

/**
 * 保存菜单。
 */
async function submitMenu(): Promise<void> {
  saving.value = true;
  try {
    const payload = {
      ...menuForm,
      parentId: menuForm.parentId || null,
      path: menuForm.path || null,
      component: menuForm.component || null,
      icon: menuForm.icon || null,
      permissionId: menuForm.permissionId || null
    };

    if (editingMenuId.value) {
      await updateMenu(editingMenuId.value, payload);
    } else {
      await createMenu(payload);
    }
    ElMessage.success('保存成功');
    menuDialogVisible.value = false;
    await loadData();
  } finally {
    saving.value = false;
  }
}

onMounted(loadData);
</script>
