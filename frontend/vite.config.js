import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";

export default defineConfig({
  plugins: [react()],
  server: {
    // Original frontend development port.
    // API calls stay aligned through the proxy rather than through
    // browser-visible hardcoded backend ports.
    host: "localhost",
    port: 5173,
    strictPort: true,
    proxy: {
      "/api": {
        target: "http://localhost:5267",
        changeOrigin: true,
      },
    },
  },
});
