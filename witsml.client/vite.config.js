import { fileURLToPath, URL } from 'node:url';
import { defineConfig } from 'vite';
import vue from '@vitejs/plugin-vue';

const target = process.env.ASPNETCORE_HTTPS_PORT
  ? `https://localhost:${process.env.ASPNETCORE_HTTPS_PORT}`
  : process.env.ASPNETCORE_URLS
    ? process.env.ASPNETCORE_URLS.split(';')[0]
    : 'http://localhost:5235';

export default defineConfig({
  plugins: [vue()],
  resolve: {
    alias: { '@': fileURLToPath(new URL('./src', import.meta.url)) }
  },
  server: {
    proxy: {
      '^/api': { target, changeOrigin: true, secure: false }
    },
    port: parseInt(process.env.DEV_SERVER_PORT || '50802')
  }
});
