import {defineConfig} from "vite";
import vue from "@vitejs/plugin-vue";

export default defineConfig({
  plugins: [
    vue({
      template: {
        compilerOptions: {
          isCustomElement: (tag) => ["md-linedivider"].includes(tag),
        },
      },
    }),
  ],

  server: {
    // host: "0.0.0.0",
    // port: parseInt(process.env.PORT) || 5173,

    proxy: {
      "/api": {
        target: "http://localhost:7174",
        changeOrigin: true,
        secure: false,
      },
    },
  },
});
