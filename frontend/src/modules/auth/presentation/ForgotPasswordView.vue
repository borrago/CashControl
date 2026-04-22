<template>
  <BaseCard title="Recuperar acesso" subtitle="Dispara o endpoint de solicitação de reset sem expor o token na API.">
    <form class="form" @submit.prevent="handleSubmit">
      <StatusBanner :message="statusMessage" :tone="statusTone" />
      <BaseInput v-model="email" label="E-mail" type="email" placeholder="voce@empresa.com" />
      <BaseButton type="submit" :loading="isSubmitting">Solicitar redefinição</BaseButton>
    </form>
  </BaseCard>
</template>

<script setup lang="ts">
import { ref } from "vue";
import { ApiClientError } from "@/shared/api/api-error";
import { useSessionStore } from "@/modules/auth/application/session.store";
import BaseButton from "@/shared/ui/BaseButton.vue";
import BaseCard from "@/shared/ui/BaseCard.vue";
import BaseInput from "@/shared/ui/BaseInput.vue";
import StatusBanner from "@/shared/ui/StatusBanner.vue";

const sessionStore = useSessionStore();
const email = ref("");
const isSubmitting = ref(false);
const statusMessage = ref("");
const statusTone = ref<"info" | "success" | "error">("info");

async function handleSubmit() {
  try {
    isSubmitting.value = true;
    await sessionStore.forgotPassword({ email: email.value });
    statusTone.value = "success";
    statusMessage.value = "Se a conta existir e estiver confirmada, o e-mail de reset sera enviado.";
  } catch (error) {
    statusTone.value = "error";
    statusMessage.value = error instanceof ApiClientError ? error.message : "Falha ao solicitar reset.";
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
