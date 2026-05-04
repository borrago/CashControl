<template>
  <BaseCard title="Recuperar acesso" subtitle="Dispara o endpoint de solicitação de reset sem expor o token na API.">
    <form class="form" @submit.prevent="handleSubmit">
      <BaseInput v-model="email" label="E-mail" type="email" placeholder="nome@empresa.com" />
      <BaseButton type="submit" :loading="isSubmitting">Solicitar redefinição</BaseButton>
    </form>
  </BaseCard>
</template>

<script setup lang="ts">
import { ref } from "vue";
import { useSessionStore } from "@/modules/auth/application/session.store";
import BaseButton from "@/shared/ui/BaseButton.vue";
import BaseCard from "@/shared/ui/BaseCard.vue";
import BaseInput from "@/shared/ui/BaseInput.vue";
import { useToasts } from "@/shared/ui/toast.store";

const sessionStore = useSessionStore();
const { notifyError, notifyInfo, getErrorMessage } = useToasts();
const email = ref("");
const isSubmitting = ref(false);

async function handleSubmit() {
  try {
    isSubmitting.value = true;
    await sessionStore.forgotPassword({ email: email.value });
    notifyInfo("Se a conta existir e estiver confirmada, o e-mail de reset será enviado.");
  } catch (error) {
    notifyError(getErrorMessage(error, "Falha ao solicitar reset."));
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
