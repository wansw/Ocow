import vue from '@vitejs/plugin-vue';
import { defineConfig, loadEnv } from 'vite';

export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), '');

  return {
    plugins: [vue()],
    build: {
      chunkSizeWarningLimit: 1000,
      rollupOptions: {
        // 过滤依赖包中的无害 PURE 注释提示，保留业务代码相关构建警告。
        onwarn(warning, warn) {
          if (warning.code === 'INVALID_ANNOTATION' && warning.id?.includes('node_modules')) {
            return;
          }

          warn(warning);
        },
        output: {
          manualChunks: {
            vue: ['vue', 'vue-router'],
            element: ['element-plus'],
            lucide: ['lucide-vue-next']
          }
        }
      }
    },
    server: {
      proxy: {
        '/api': {
          target: env.VITE_GATEWAY_PROXY_TARGET || 'http://localhost:5000',
          changeOrigin: true
        }
      }
    }
  };
});
