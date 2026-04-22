<template>
  <BaseCard title="Redefinir senha" subtitle="Use o token recebido por e-mail para concluir o reset.">
    <form class="form" @submit.prevent="handleSubmit">
      <StatusBanner :message="statusMessage" :tone="statusTone" />
      <BaseInput v-model="form.email" label="E-mail" type="email" />
      <BaseInput v-model="form.token" label="Token" />
      <BaseInput v-model="form.newPassword" label="Nova senha" type="password" />
      <BaseButton type="submit" :loading="isSubmitting">Atualizar senha</BaseButton>
    </form>
  </BaseCard>
</template>

<script setup lang="ts">
import { reactive, ref } from "vue";
import { useRoute } from "vue-router";
import { ApiClientError } from "@/shared/api/api-error";
import { useSessionStore } from "@/modules/auth/application/session.store";
import BaseButton from "@/shared/ui/BaseButton.vue";
import BaseCard from "@/shared/ui/BaseCard.vue";
import BaseInput from "@/shared/ui/BaseInput.vue";
import StatusBanner from "@/shared/ui/StatusBanner.vue";

const route = useRoute();
const sessionStore = useSessionStore();

const form = reactive({
  email: typeof route.query.email === "string" ? route.query.email : "",
  token: typeof route.query.token === "string" ? route.query.token : "",
  newPassword: "",
});

const isSubmitting = ref(false);
const statusMessage = ref("");
const statusTone = ref<"info" | "success" | "error">("info");

async function handleSubmit() {
  try {
    isSubmitting.value = true;
    await sessionStore.resetPassword(form);
    statusTone.value = "success";
    statusMessage.value = "Senha redefinida com sucesso.";
  } catch (error) {
    statusTone.value = "error";
    statusMessage.value = error instanceof ApiClientError ? error.message : "Falha ao redefinir senha.";
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
</style>
