<template>
  <BaseCard title="Entrar" subtitle="Usa login por cookie de sessão e leitura do usuário autenticado.">
    <form class="form" @submit.prevent="handleSubmit">
      <BaseInput v-model="form.email" label="E-mail" type="email" placeholder="nome@empresa.com" />
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
import { RouterLink, useRouter } from "vue-router";
import { useSessionStore } from "@/modules/auth/application/session.store";
import BaseButton from "@/shared/ui/BaseButton.vue";
import BaseCard from "@/shared/ui/BaseCard.vue";
import BaseInput from "@/shared/ui/BaseInput.vue";
import { useToasts } from "@/shared/ui/toast.store";

const router = useRouter();
const sessionStore = useSessionStore();
const { notifyError, notifySuccess, getErrorMessage } = useToasts();

const form = reactive({
  email: "",
  password: "",
});

const isSubmitting = ref(false);

async function handleSubmit() {
  try {
    isSubmitting.value = true;

    await sessionStore.login(form);

    notifySuccess("Sessão iniciada com sucesso.");

    await router.push({ name: "home" });
  } catch (error) {
    notifyError(getErrorMessage(error, "Falha ao entrar."));
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
