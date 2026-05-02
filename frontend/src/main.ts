import { createApp } from "vue";
import { createPinia } from "pinia";
import App from "@/app/App.vue";
import { router } from "@/app/router";
import { useSessionStore } from "@/modules/auth/application/session.store";
import { configureHttpClient } from "@/shared/api/http-client";
import "@/shared/styles/main.css";

async function bootstrap() {
  const app = createApp(App);
  const pinia = createPinia();

  app.use(pinia);

  const sessionStore = useSessionStore(pinia);
  configureHttpClient({
    clearSession: () => sessionStore.clearSession(),
  });

  await sessionStore.bootstrap();

  app.use(router);
  await router.isReady();

  app.mount("#app");
}

void bootstrap();
