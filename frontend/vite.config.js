import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";
import react from "@vitejs/plugin-react";
import { defineConfig } from "vite";

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const devCertPath = path.resolve(__dirname, ".cert/localhost-cert.pem");
const devKeyPath = path.resolve(__dirname, ".cert/localhost-key.pem");

export default defineConfig(({ command }) => {
  const useHttps = command === "serve";

  if (useHttps && (!fs.existsSync(devCertPath) || !fs.existsSync(devKeyPath))) {
    throw new Error(
      "Missing frontend HTTPS certificate files. Generate frontend/.cert/localhost-cert.pem and frontend/.cert/localhost-key.pem before running Vite.",
    );
  }

  return {
    plugins: [react()],
    server: {
      host: "localhost",
      port: 5173,
      strictPort: true,
      https: useHttps
        ? {
            cert: fs.readFileSync(devCertPath),
            key: fs.readFileSync(devKeyPath),
          }
        : undefined,
      proxy: {
        "/api": {
          target: "https://localhost:7117",
          changeOrigin: true,
          secure: false,
        },
      },
    },
  };
});
