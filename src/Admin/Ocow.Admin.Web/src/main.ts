import ElementPlus from 'element-plus';
import 'element-plus/dist/index.css';
import { ElMessage } from 'element-plus';
import { createApp } from 'vue';
import App from './App.vue';
import router from './router';
import './styles/app.css';

const app = createApp(App);

// 统一处理 Vue 事件和渲染中的异常，避免接口错误静默失败。
app.config.errorHandler = (error) => {
  const message = error instanceof Error ? error.message : '操作失败';
  ElMessage.error(message);
};

app.use(router)
  .use(ElementPlus)
  .mount('#app');
