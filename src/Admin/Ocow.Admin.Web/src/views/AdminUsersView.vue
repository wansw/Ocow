<template>
  <main class="page">
    <div class="toolbar">
      <h2 class="toolbar-title">后台人员</h2>
      <div class="toolbar-actions">
        <el-button :icon="RefreshCw" @click="loadUsers">刷新</el-button>
        <el-button v-if="canCreate" :icon="Plus" type="primary" @click="openCreateDialog">新增</el-button>
      </div>
    </div>

    <section class="content-panel">
      <el-table v-loading="loading" :data="users" row-key="id">
        <el-table-column prop="userName" label="账号" min-width="160" />
        <el-table-column prop="displayName" label="名称" min-width="160" />
        <el-table-column label="状态" width="110">
          <template #default="{ row }">
            <el-tag :type="row.status === 1 ? 'success' : 'info'">{{ formatAdminStatus(row.status) }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column label="编号" min-width="260">
          <template #default="{ row }">
            <span class="muted">{{ row.id }}</span>
          </template>
        </el-table-column>
        <el-table-column label="操作" width="150" fixed="right">
          <template #default="{ row }">
            <div class="table-actions">
              <el-button
                v-if="canDisable && row.status === 1"
                :icon="Ban"
                size="small"
                type="danger"
                @click="disableUser(row)"
              >
                禁用
              </el-button>
            </div>
          </template>
        </el-table-column>
      </el-table>

      <el-pagination
        v-model:current-page="page.pageIndex"
        v-model:page-size="page.pageSize"
        :total="total"
        :page-sizes="[10, 20, 50]"
        layout="total, sizes, prev, pager, next"
        style="margin-top: 14px; justify-content: flex-end"
        @change="loadUsers"
      />
    </section>

    <el-dialog v-model="createDialogVisible" title="新增后台人员" width="520px">
      <el-form :model="createForm" label-position="top">
        <div class="form-grid">
          <el-form-item label="账号">
            <el-input v-model="createForm.userName" />
          </el-form-item>
          <el-form-item label="名称">
            <el-input v-model="createForm.displayName" />
          </el-form-item>
          <el-form-item label="初始密码" class="full">
            <el-input v-model="createForm.password" show-password type="password" />
          </el-form-item>
          <el-form-item label="角色" class="full">
            <el-select v-model="createForm.roleIds" multiple style="width: 100%">
              <el-option v-for="role in roles" :key="role.id" :label="role.name" :value="role.id" />
            </el-select>
          </el-form-item>
        </div>
      </el-form>
      <template #footer>
        <el-button @click="createDialogVisible = false">取消</el-button>
        <el-button :loading="saving" type="primary" @click="submitCreate">保存</el-button>
      </template>
    </el-dialog>
  </main>
</template>

<script setup lang="ts">
import { Ban, Plus, RefreshCw } from 'lucide-vue-next';
import { ElMessage, ElMessageBox } from 'element-plus';
import { computed, onMounted, reactive, ref } from 'vue';
import { createAdminUser, disableAdminUser, getAdminUsers, getRoles } from '../api/identity';
import type { AdminUser, Role } from '../types/admin';
import { formatAdminStatus } from '../utils/format';
import { hasPermission } from '../utils/permissions';

const loading = ref(false);
const saving = ref(false);
const users = ref<AdminUser[]>([]);
const roles = ref<Role[]>([]);
const total = ref(0);
const createDialogVisible = ref(false);
const page = reactive({
  pageIndex: 1,
  pageSize: 20
});
const createForm = reactive({
  userName: '',
  password: '',
  displayName: '',
  roleIds: [] as string[]
});

const canCreate = computed(() => hasPermission('identity.admin-user.create'));
const canDisable = computed(() => hasPermission('identity.admin-user.disable'));

/**
 * 查询后台人员列表。
 */
async function loadUsers(): Promise<void> {
  loading.value = true;
  try {
    const result = await getAdminUsers(page);
    users.value = result.items;
    total.value = result.total;
  } finally {
    loading.value = false;
  }
}

/**
 * 查询角色下拉数据。
 */
async function loadRoles(): Promise<void> {
  roles.value = await getRoles();
}

/**
 * 打开新增后台人员弹窗。
 */
function openCreateDialog(): void {
  createForm.userName = '';
  createForm.password = '';
  createForm.displayName = '';
  createForm.roleIds = [];
  createDialogVisible.value = true;
}

/**
 * 提交新增后台人员。
 */
async function submitCreate(): Promise<void> {
  saving.value = true;
  try {
    await createAdminUser(createForm);
    ElMessage.success('保存成功');
    createDialogVisible.value = false;
    await loadUsers();
  } finally {
    saving.value = false;
  }
}

/**
 * 禁用指定后台人员。
 */
async function disableUser(user: AdminUser): Promise<void> {
  await ElMessageBox.confirm(`确认禁用 ${user.displayName || user.userName}？`, '禁用账号', {
    type: 'warning'
  });
  await disableAdminUser(user.id);
  ElMessage.success('已禁用');
  await loadUsers();
}

onMounted(async () => {
  await Promise.all([loadUsers(), loadRoles()]);
});
</script>
