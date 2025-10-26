import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

const basePath = process.env.VITE_BASE ?? "/BenchBoss/";
const outDir = process.env.VITE_OUT_DIR ?? "../docs";

export default defineConfig(({ mode }) => {
  return {
    plugins: [react()],
    base: basePath.endsWith("/") ? basePath : `${basePath}/`,
    root: "./src",
    build: {
      outDir,
      emptyOutDir: true,
      sourcemap: mode === "development",
    },
    server: {
      port: 8080,
    }
  };
});
