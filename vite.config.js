import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig(({ mode }) => {
  return {
    plugins: [react()],
    base: "/BenchBoss/",
    root: "./src",
    build: {
      outDir: "../build",
      emptyOutDir: true,
      sourcemap: mode === "development",
    },
    server: {
      port: 8080,
    }
  };
});
