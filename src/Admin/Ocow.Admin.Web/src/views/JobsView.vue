<template>
  <main class="page">
    <div class="toolbar">
      <h2 class="toolbar-title">任务管理</h2>
      <div class="toolbar-actions">
        <el-button :icon="PanelTopOpen" @click="openDashboard">Dashboard</el-button>
        <el-button :icon="Play" type="primary" @click="triggerSample">触发示例</el-button>
      </div>
    </div>

    <section class="content-panel">
      <el-form :model="jobForm" label-position="top">
        <div class="form-grid">
          <el-form-item label="任务名称">
            <el-input v-model="jobForm.jobName" />
          </el-form-item>
          <el-form-item label="任务类型">
            <el-input v-model="jobForm.jobType" />
          </el-form-item>
          <el-form-item label="Cron">
            <el-input v-model="jobForm.cron" />
          </el-form-item>
          <el-form-item label="HTTP 方法">
            <el-select v-model="jobForm.httpMethod" style="width: 100%">
              <el-option label="GET" value="GET" />
              <el-option label="POST" value="POST" />
              <el-option label="PUT" value="PUT" />
              <el-option label="DELETE" value="DELETE" />
            </el-select>
          </el-form-item>
          <el-form-item label="目标服务">
            <el-input v-model="jobForm.targetService" />
          </el-form-item>
          <el-form-item label="目标接口">
            <el-input v-model="jobForm.targetApi" />
          </el-form-item>
          <el-form-item label="请求体 JSON" class="full">
            <el-input v-model="jobForm.requestBody" :rows="5" type="textarea" />
          </el-form-item>
          <el-form-item label="启用">
            <el-switch v-model="jobForm.enabled" />
          </el-form-item>
        </div>
      </el-form>

      <div class="toolbar" style="margin: 8px 0 0">
        <span class="muted">最近任务：{{ lastDefinition?.jobName || '-' }}</span>
        <div class="toolbar-actions">
          <el-button :loading="saving" :icon="Save" type="primary" @click="submitJobDefinition">保存任务</el-button>
          <el-button
            :disabled="!lastDefinition"
            :loading="triggering"
            :icon="Play"
            type="success"
            @click="triggerLastDefinition"
          >
            触发任务
          </el-button>
        </div>
      </div>
    </section>

    <section v-if="lastResult" class="content-panel" style="margin-top: 16px">
      <el-descriptions :column="2" border>
        <el-descriptions-item label="结果类型">{{ lastResult.type }}</el-descriptions-item>
        <el-descriptions-item label="后台任务编号">{{ lastResult.backgroundJobId }}</el-descriptions-item>
        <el-descriptions-item label="任务名称">{{ lastResult.name }}</el-descriptions-item>
        <el-descriptions-item label="任务定义">{{ lastResult.definitionId }}</el-descriptions-item>
      </el-descriptions>
    </section>
  </main>
</template>

<script setup lang="ts">
import { PanelTopOpen, Play, Save } from 'lucide-vue-next';
import { ElMessage } from 'element-plus';
import { reactive, ref } from 'vue';
import {
  createDashboardSession,
  enqueueSampleJob,
  saveJobDefinition,
  triggerJobDefinition
} from '../api/jobs';
import type { CreateJobDefinitionReqDto, JobDefinition } from '../types/admin';

const LAST_JOB_KEY = 'ocow.admin.last.job-definition';
const saving = ref(false);
const triggering = ref(false);
const lastDefinition = ref<JobDefinition | null>(readLastDefinition());
const lastResult = ref<{
  type: string;
  backgroundJobId: string;
  name: string;
  definitionId: string;
} | null>(null);
const jobForm = reactive<CreateJobDefinitionReqDto>({
  jobName: '订单同步任务',
  jobType: 'Http',
  cron: '*/1 * * * *',
  targetService: 'Order',
  targetApi: '/internal/orders/sync/erp',
  httpMethod: 'POST',
  requestBody: '{}',
  enabled: true
});

/**
 * 从本地存储读取最近创建的任务定义。
 */
function readLastDefinition(): JobDefinition | null {
  const raw = localStorage.getItem(LAST_JOB_KEY);
  if (!raw) {
    return null;
  }

  try {
    return JSON.parse(raw) as JobDefinition;
  } catch {
    localStorage.removeItem(LAST_JOB_KEY);
    return null;
  }
}

/**
 * 保存最近创建的任务定义。
 */
function writeLastDefinition(definition: JobDefinition): void {
  lastDefinition.value = definition;
  localStorage.setItem(LAST_JOB_KEY, JSON.stringify(definition));
}

/**
 * 创建 Hangfire Dashboard 会话并打开面板。
 */
async function openDashboard(): Promise<void> {
  const session = await createDashboardSession();
  window.open(session.dashboardPath || '/hangfire', '_blank', 'noopener,noreferrer');
}

/**
 * 手动触发示例后台任务。
 */
async function triggerSample(): Promise<void> {
  triggering.value = true;
  try {
    const result = await enqueueSampleJob();
    lastResult.value = {
      type: 'sample',
      backgroundJobId: result.jobId,
      name: result.name,
      definitionId: '-'
    };
    ElMessage.success('已触发');
  } finally {
    triggering.value = false;
  }
}

/**
 * 保存动态任务定义。
 */
async function submitJobDefinition(): Promise<void> {
  saving.value = true;
  try {
    const definition = await saveJobDefinition({
      ...jobForm,
      requestBody: jobForm.requestBody || null
    });
    writeLastDefinition(definition);
    ElMessage.success('保存成功');
  } finally {
    saving.value = false;
  }
}

/**
 * 手动触发最近保存的动态任务。
 */
async function triggerLastDefinition(): Promise<void> {
  if (!lastDefinition.value) {
    return;
  }

  triggering.value = true;
  try {
    const result = await triggerJobDefinition(lastDefinition.value.id);
    lastResult.value = {
      type: 'definition',
      backgroundJobId: result.backgroundJobId,
      name: lastDefinition.value.jobName,
      definitionId: lastDefinition.value.id
    };
    ElMessage.success('已触发');
  } finally {
    triggering.value = false;
  }
}
</script>
