<template>
  <BaseCard title="Entrar" subtitle="Usa os endpoints de login, refresh automático e leitura do usuário autenticado.">
    <form class="form" @submit.prevent="handleSubmit">
      <StatusBanner :message="statusMessage" :tone="statusTone" />

      <BaseInput v-model="form.email" label="E-mail" type="email" placeholder="voce@empresa.com" />
      <BaseInput v-model="form.password" label="Senha" type="password" placeholder="Sua senha" />

      <div class="actions">
        <BaseButton type="submit" :loading="isSubmitting">Entrar</BaseButton>
        <RouterLink to="/forgot-password">Esqueci minha senha</RouterLink>
      </div>
    </form>
  </BaseCard>
</template>

<script setup lang="ts">
import { reactive, ref } from "vue";
import { RouterLink, useRoute, useRouter } from "vue-router";
import { ApiClientError } from "@/shared/api/api-error";
import { useSessionStore } from "@/modules/auth/application/session.store";
import BaseButton from "@/shared/ui/BaseButton.vue";
import BaseCard from "@/shared/ui/BaseCard.vue";
import BaseInput from "@/shared/ui/BaseInput.vue";
import StatusBanner from "@/shared/ui/StatusBanner.vue";

const router = useRouter();
const route = useRoute();
const sessionStore = useSessionStore();

const form = reactive({
  email: "",
  password: "",
});

const isSubmitting = ref(false);
const statusMessage = ref("");
const statusTone = ref<"info" | "success" | "error">("info");

async function handleSubmit() {
  try {
    isSubmitting.value = true;
    statusMessage.value = "";

    await sessionStore.login(form);

    statusTone.value = "success";
    statusMessage.value = "Sessao iniciada com sucesso.";

    const redirect = typeof route.query.redirect === "string" ? route.query.redirect : "/profile";
    await router.push(redirect);
  } catch (error) {
    statusTone.value = "error";
    statusMessage.value = error instanceof ApiClientError ? error.message : "Falha ao entrar.";
  } finally {
    isSubmitting.value = false;
  }
}
</script>

<style scoped>
.form {
  display: grid;
  gap: 1rem;
}

.actions {
  display: flex;
  justify-content: space-between;
  gap: 1rem;
  align-items: center;
  flex-wrap: wrap;
}
</style>
