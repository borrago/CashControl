import { existsSync, readFileSync } from "node:fs";
import { fileURLToPath, URL } from "node:url";
import { defineConfig } from "vite";
import vue from "@vitejs/plugin-vue";

const httpsCertificatePath = fileURLToPath(new URL("./.certs/localhost.pfx", import.meta.url));
const useHttps = process.env.VITE_DEV_HTTPS === "true";

export default defineConfig({
  plugins: [vue()],
  server: useHttps
    ? {
        host: "localhost",
        port: 5173,
        strictPort: true,
        https: existsSync(httpsCertificatePath)
          ? {
              pfx: readFileSync(httpsCertificatePath),
              passphrase: "cashcontrol-dev",
            }
          : undefined,
      }
    : undefined,
  resolve: {
    alias: {
      "@": fileURLToPath(new URL("./src", import.meta.url)),
    },
  },
  test: {
    environment: "jsdom",
    globals: true,
    setupFiles: "./src/test/setup.ts",
    coverage: {
      reporter: ["text", "html"],
    },
  },
});
