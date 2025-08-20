import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig({
  plugins: [react()],
  base: "/BenchBoss/",
  root: "./src",
  build: {
    outDir: "../docs",
    emptyOutDir: true,
  },
  server: {
    port: 8080,
  },
})
