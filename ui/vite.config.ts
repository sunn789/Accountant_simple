import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    port: 3000,
    proxy: {
      '/api': {
        target: 'http://localhost:5133', // آدرس بک‌اند
        changeOrigin: true,
        secure: false
      },
    },
  },
    optimizeDeps: {
    include: ['react-multi-date-picker', 'react-date-object'],
  },
})

