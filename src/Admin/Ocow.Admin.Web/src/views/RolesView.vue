<template>
  <main class="page">
    <div class="toolbar">
      <h2 class="toolbar-title">角色管理</h2>
      <div class="toolbar-actions">
        <el-button :icon="RefreshCw" @click="loadData">刷新</el-button>
        <el-button v-if="canSave" :icon="Plus" type="primary" @click="openRoleDialog()">新增</el-button>
      </div>
    </div>

    <section class="content-panel">
      <el-table v-loading="loading" :data="roles" row-key="id">
        <el-table-column prop="code" label="角色编码" min-width="180" />
        <el-table-column prop="name" label="角色名称" min-width="180" />
        <el-table-column label="编号" min-width="260">
          <template #default="{ row }">
            <span class="muted">{{ row.id }}</span>
          </template>
        </el-table-column>
        <el-table-column label="操作" width="230" fixed="right">
          <template #default="{ row }">
            <div class="table-actions">
              <el-button v-if="canSave" :icon="Pencil" size="small" @click="openRoleDialog(row)">编辑</el-button>
              <el-button
                v-if="canBind"
                :icon="KeyRound"
                size="small"
                type="primary"
                @click="openPermissionDialog(row)"
              >
                权限
              </el-button>
            </div>
          </template>
        </el-table-column>
      </el-table>
    </section>

    <el-dialog v-model="roleDialogVisible" :title="editingRoleId ? '编辑角色' : '新增角色'" width="460px">
      <el-form :model="roleForm" label-position="top">
        <el-form-item label="角色编码">
          <el-input v-model="roleForm.code" />
        </el-form-item>
        <el-form-item label="角色名称">
          <el-input v-model="roleForm.name" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="roleDialogVisible = false">取消</el-button>
        <el-button :loading="saving" type="primary" @click="submitRole">保存</el-button>
      </template>
    </el-dialog>

    <el-dialog v-model="permissionDialogVisible" title="绑定权限" width="680px">
      <el-scrollbar max-height="430px">
        <div v-for="group in permissionGroups" :key="group.module" style="margin-bottom: 16px">
          <el-divider content-position="left">{{ group.module }}</el-divider>
          <el-checkbox-group v-model="selectedPermissionIds">
            <el-checkbox
              v-for="permission in group.permissions"
              :key="permission.id"
              :label="permission.id"
              border
              style="margin: 0 8px 8px 0"
            >
              {{ permission.name }}（{{ permission.code }}）
            </el-checkbox>
          </el-checkbox-group>
        </div>
      </el-scrollbar>
      <template #footer>
        <el-button @click="permissionDialogVisible = false">取消</el-button>
        <el-button :loading="saving" type="primary" @click="submitBindPermissions">保存</el-button>
      </template>
    </el-dialog>
  </main>
</template>

<script setup lang="ts">
import { KeyRound, Pencil, Plus, RefreshCw } from 'lucide-vue-next';
import { ElMessage } from 'element-plus';
import { computed, onMounted, reactive, ref } from 'vue';
import { bindRolePermissions, createRole, getPermissions, getRoles, updateRole } from '../api/identity';
import type { Permission, Role } from '../types/admin';
import { hasPermission } from '../utils/permissions';

const loading = ref(false);
const saving = ref(false);
const roles = ref<Role[]>([]);
const permissions = ref<Permission[]>([]);
const roleDialogVisible = ref(false);
const permissionDialogVisible = ref(false);
const editingRoleId = ref<string | null>(null);
const bindingRoleId = ref<string | null>(null);
const selectedPermissionIds = ref<string[]>([]);
const roleForm = reactive({
  code: '',
  name: ''
});

const canSave = computed(() => hasPermission('identity.role.save'));
const canBind = computed(() => hasPermission('identity.role.bind-permission'));
const permissionGroups = computed(() => {
  const map = new Map<string, Permission[]>();
  for (const permission of permissions.value) {
    const group = map.get(permission.module) ?? [];
    group.push(permission);
    map.set(permission.module, group);
  }

  return Array.from(map.entries()).map(([module, items]) => ({
    module,
    permissions: items.sort((a, b) => a.code.localeCompare(b.code))
  }));
});

/**
 * 查询角色和权限点数据。
 */
async function loadData(): Promise<void> {
  loading.value = true;
  try {
    const [roleResult, permissionResult] = await Promise.all([getRoles(), getPermissions()]);
    roles.value = roleResult;
    permissions.value = permissionResult;
  } finally {
    loading.value = false;
  }
}

/**
 * 打开角色新增或编辑弹窗。
 */
function openRoleDialog(role?: Role): void {
  editingRoleId.value = role?.id ?? null;
  roleForm.code = role?.code ?? '';
  roleForm.name = role?.name ?? '';
  roleDialogVisible.value = true;
}

/**
 * 保存角色。
 */
async function submitRole(): Promise<void> {
  saving.value = true;
  try {
    if (editingRoleId.value) {
      await updateRole(editingRoleId.value, roleForm);
    } else {
      await createRole(roleForm);
    }
    ElMessage.success('保存成功');
    roleDialogVisible.value = false;
    await loadData();
  } finally {
    saving.value = false;
  }
}

/**
 * 打开角色权限绑定弹窗。
 */
function openPermissionDialog(role: Role): void {
  bindingRoleId.value = role.id;
  selectedPermissionIds.value = [];
  permissionDialogVisible.value = true;
}

/**
 * 保存角色权限点绑定。
 */
async function submitBindPermissions(): Promise<void> {
  if (!bindingRoleId.value) {
    return;
  }

  saving.value = true;
  try {
    await bindRolePermissions(bindingRoleId.value, { permissionIds: selectedPermissionIds.value });
    ElMessage.success('保存成功');
    permissionDialogVisible.value = false;
  } finally {
    saving.value = false;
  }
}

onMounted(loadData);
</script>
