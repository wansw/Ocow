<template>
  <main class="page">
    <div class="toolbar">
      <h2 class="toolbar-title">订单管理</h2>
      <div class="toolbar-actions">
        <el-button :icon="RefreshCw" @click="loadOrders">刷新</el-button>
      </div>
    </div>

    <section class="content-panel">
      <el-table v-loading="loading" :data="orders" row-key="id">
        <el-table-column prop="orderNo" label="订单号" min-width="180" />
        <el-table-column label="状态" width="110">
          <template #default="{ row }">
            <el-tag :type="orderTagType(row.status)">{{ formatOrderStatus(row.status) }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column label="金额" width="130">
          <template #default="{ row }">{{ formatMoney(row.totalAmount) }}</template>
        </el-table-column>
        <el-table-column label="下单会员" min-width="250">
          <template #default="{ row }">
            <span class="muted">{{ row.customerId }}</span>
          </template>
        </el-table-column>
        <el-table-column label="创建时间" min-width="180">
          <template #default="{ row }">{{ formatDateTime(row.createdAt) }}</template>
        </el-table-column>
        <el-table-column label="操作" width="190" fixed="right">
          <template #default="{ row }">
            <div class="table-actions">
              <el-button :icon="Eye" size="small" @click="openDetail(row.id)">详情</el-button>
              <el-button
                v-if="canShip && row.status === 2"
                :icon="Truck"
                size="small"
                type="primary"
                @click="openShipDialog(row)"
              >
                发货
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
        @change="loadOrders"
      />
    </section>

    <el-drawer v-model="detailVisible" title="订单详情" size="420px">
      <el-descriptions v-if="detail" :column="1" border>
        <el-descriptions-item label="订单号">{{ detail.orderNo }}</el-descriptions-item>
        <el-descriptions-item label="状态">{{ formatOrderStatus(detail.status) }}</el-descriptions-item>
        <el-descriptions-item label="金额">{{ formatMoney(detail.totalAmount) }}</el-descriptions-item>
        <el-descriptions-item label="会员">{{ detail.customerId }}</el-descriptions-item>
        <el-descriptions-item label="创建时间">{{ formatDateTime(detail.createdAt) }}</el-descriptions-item>
        <el-descriptions-item label="订单编号">{{ detail.id }}</el-descriptions-item>
      </el-descriptions>
    </el-drawer>

    <el-dialog v-model="shipDialogVisible" title="订单发货" width="460px">
      <el-form :model="shipForm" label-position="top">
        <el-form-item label="物流公司">
          <el-input v-model="shipForm.expressCompany" />
        </el-form-item>
        <el-form-item label="物流单号">
          <el-input v-model="shipForm.expressNo" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="shipDialogVisible = false">取消</el-button>
        <el-button :loading="saving" type="primary" @click="submitShip">保存</el-button>
      </template>
    </el-dialog>
  </main>
</template>

<script setup lang="ts">
import { Eye, RefreshCw, Truck } from 'lucide-vue-next';
import { ElMessage } from 'element-plus';
import { computed, onMounted, reactive, ref } from 'vue';
import { getOrder, getOrders, shipOrder } from '../api/orders';
import type { Order } from '../types/admin';
import { formatDateTime, formatMoney, formatOrderStatus } from '../utils/format';
import { hasPermission } from '../utils/permissions';

const loading = ref(false);
const saving = ref(false);
const orders = ref<Order[]>([]);
const detail = ref<Order | null>(null);
const total = ref(0);
const selectedOrderId = ref<string | null>(null);
const detailVisible = ref(false);
const shipDialogVisible = ref(false);
const page = reactive({
  pageIndex: 1,
  pageSize: 20
});
const shipForm = reactive({
  expressCompany: '',
  expressNo: ''
});

const canShip = computed(() => hasPermission('order.ship'));

/**
 * 查询订单分页列表。
 */
async function loadOrders(): Promise<void> {
  loading.value = true;
  try {
    const result = await getOrders(page);
    orders.value = result.items;
    total.value = result.total;
  } finally {
    loading.value = false;
  }
}

/**
 * 打开订单详情抽屉。
 */
async function openDetail(id: string): Promise<void> {
  detail.value = await getOrder(id);
  detailVisible.value = true;
}

/**
 * 打开发货弹窗。
 */
function openShipDialog(order: Order): void {
  selectedOrderId.value = order.id;
  shipForm.expressCompany = '';
  shipForm.expressNo = '';
  shipDialogVisible.value = true;
}

/**
 * 提交订单发货。
 */
async function submitShip(): Promise<void> {
  if (!selectedOrderId.value) {
    return;
  }

  saving.value = true;
  try {
    await shipOrder(selectedOrderId.value, shipForm);
    ElMessage.success('发货成功');
    shipDialogVisible.value = false;
    await loadOrders();
  } finally {
    saving.value = false;
  }
}

/**
 * 返回订单状态标签类型。
 */
function orderTagType(status: number): 'primary' | 'success' | 'warning' | 'info' | 'danger' {
  const typeMap: Record<number, 'primary' | 'success' | 'warning' | 'info' | 'danger'> = {
    1: 'warning',
    2: 'primary',
    3: 'success',
    4: 'success',
    5: 'info'
  };
  return typeMap[status] ?? 'info';
}

onMounted(loadOrders);
</script>
