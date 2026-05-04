import { createApp } from "vue";
import { createPinia } from "pinia";
import App from "@/app/App.vue";
import { router } from "@/app/router";
import { useSessionStore } from "@/modules/auth/application/session.store";
import { configureHttpClient } from "@/shared/api/http-client";
import { registerServiceWorker } from "@/shared/pwa/register-service-worker";
import "@/shared/styles/main.css";

async function bootstrap() {
  const app = createApp(App);
  const pinia = createPinia();

  app.use(pinia);

  const sessionStore = useSessionStore(pinia);
  configureHttpClient({
    clearSession: () => sessionStore.clearSession(),
  });

  app.use(router);
  app.mount("#app");
  registerServiceWorker();
}

void bootstrap();
