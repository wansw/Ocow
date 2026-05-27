<template>
  <main class="login-page">
    <section class="login-panel">
      <h1>Ocow 后台管理</h1>
      <p>使用管理员账号登录</p>

      <el-form :model="form" label-position="top" @submit.prevent="handleLogin">
        <el-form-item label="账号">
          <el-input v-model="form.userName" autocomplete="username" size="large" />
        </el-form-item>
        <el-form-item label="密码">
          <el-input
            v-model="form.password"
            autocomplete="current-password"
            show-password
            size="large"
            type="password"
          />
        </el-form-item>
        <el-button
          :loading="loading"
          :icon="LogIn"
          native-type="submit"
          size="large"
          type="primary"
          style="width: 100%"
          @click="handleLogin"
        >
          登录
        </el-button>
      </el-form>
    </section>
  </main>
</template>

<script setup lang="ts">
import { LogIn } from 'lucide-vue-next';
import { ElMessage } from 'element-plus';
import { reactive, ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { loginAdmin } from '../api/auth';
import { saveAuthSession } from '../utils/authStorage';

const route = useRoute();
const router = useRouter();
const loading = ref(false);
const form = reactive({
  userName: 'admin',
  password: 'Admin@123456'
});

/**
 * 登录后台并保存 Admin Token 会话。
 */
async function handleLogin(): Promise<void> {
  if (loading.value) {
    return;
  }

  loading.value = true;
  try {
    const token = await loginAdmin(form);
    saveAuthSession({
      accessToken: token.accessToken,
      refreshToken: token.refreshToken,
      expiresAt: token.expiresAt,
      permissions: token.permissions
    });
    ElMessage.success('登录成功');
    await router.replace(String(route.query.redirect || '/'));
  } finally {
    loading.value = false;
  }
}
</script>
