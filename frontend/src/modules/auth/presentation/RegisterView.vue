<template>
  <BaseCard title="Criar conta" subtitle="Fluxo de cadastro com confirmação de e-mail obrigatória.">
    <form class="form" @submit.prevent="handleSubmit">
      <StatusBanner :message="statusMessage" :tone="statusTone" />
      <BaseInput v-model="form.fullName" label="Nome completo" placeholder="Seu nome" />
      <BaseInput v-model="form.email" label="E-mail" type="email" placeholder="voce@empresa.com" />
      <BaseInput v-model="form.password" label="Senha" type="password" placeholder="Senha com dígito" />
      <BaseButton type="submit" :loading="isSubmitting">Cadastrar</BaseButton>
    </form>
  </BaseCard>
</template>

<script setup lang="ts">
import { reactive, ref } from "vue";
import { ApiClientError } from "@/shared/api/api-error";
import { useSessionStore } from "@/modules/auth/application/session.store";
import BaseButton from "@/shared/ui/BaseButton.vue";
import BaseCard from "@/shared/ui/BaseCard.vue";
import BaseInput from "@/shared/ui/BaseInput.vue";
import StatusBanner from "@/shared/ui/StatusBanner.vue";

const sessionStore = useSessionStore();

const form = reactive({
  fullName: "",
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

    await sessionStore.register(form);

    statusTone.value = "success";
    statusMessage.value = "Conta criada. Use o token enviado por e-mail para confirmar o cadastro.";
  } catch (error) {
    statusTone.value = "error";
    statusMessage.value = error instanceof ApiClientError ? error.message : "Falha ao cadastrar.";
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
