import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    strictPort: true,
    host: '0.0.0.0',
    allowedHosts: true,
    watch: {
      usePolling: true,
    },
    hmr: process.env.VITE_DISABLE_HMR === 'true'
      ? false
      : {
          protocol: 'ws',
          host: process.env.VITE_HMR_HOST ?? 'localhost',
          clientPort: Number(process.env.VITE_HMR_CLIENT_PORT ?? 5173),
        },
    proxy: {
      '/api': {
        target: process.env.VITE_API_URL ?? 'http://localhost:5001',
        changeOrigin: true,
      }
    }
  },
  preview: {
    port: 4173,
    host: '0.0.0.0'
  }
})
