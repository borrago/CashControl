<template>
  <BaseCard title="Criar conta" subtitle="Fluxo de cadastro com confirmação de e-mail obrigatória.">
    <form class="form" @submit.prevent="handleSubmit">
      <BaseInput v-model="form.fullName" label="Nome completo" placeholder="Seu nome" />
      <BaseInput v-model="form.email" label="E-mail" type="email" placeholder="nome@empresa.com" />
      <BaseInput v-model="form.password" label="Senha" type="password" placeholder="Senha com dígito" />
      <BaseButton type="submit" :loading="isSubmitting">Cadastrar</BaseButton>
    </form>
  </BaseCard>
</template>

<script setup lang="ts">
import { reactive, ref } from "vue";
import { useSessionStore } from "@/modules/auth/application/session.store";
import BaseButton from "@/shared/ui/BaseButton.vue";
import BaseCard from "@/shared/ui/BaseCard.vue";
import BaseInput from "@/shared/ui/BaseInput.vue";
import { useToasts } from "@/shared/ui/toast.store";

const sessionStore = useSessionStore();
const { notifyError, notifySuccess, getErrorMessage } = useToasts();

const form = reactive({
  fullName: "",
  email: "",
  password: "",
});

const isSubmitting = ref(false);

async function handleSubmit() {
  try {
    isSubmitting.value = true;

    await sessionStore.register(form);

    notifySuccess("Conta criada. Use o token enviado por e-mail para confirmar o cadastro.");
  } catch (error) {
    notifyError(getErrorMessage(error, "Falha ao cadastrar."));
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
