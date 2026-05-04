<template>
  <BaseCard title="Confirmar e-mail" subtitle="Conclui o onboarding usando o token de confirmação.">
    <form class="form" @submit.prevent="handleSubmit">
      <BaseInput v-model="form.userId" label="User Id" />
      <BaseInput v-model="form.token" label="Token" />
      <BaseButton type="submit" :loading="isSubmitting">Confirmar</BaseButton>
    </form>
  </BaseCard>
</template>

<script setup lang="ts">
import { reactive, ref } from "vue";
import { useRoute } from "vue-router";
import { useSessionStore } from "@/modules/auth/application/session.store";
import BaseButton from "@/shared/ui/BaseButton.vue";
import BaseCard from "@/shared/ui/BaseCard.vue";
import BaseInput from "@/shared/ui/BaseInput.vue";
import { useToasts } from "@/shared/ui/toast.store";

const route = useRoute();
const sessionStore = useSessionStore();
const { notifyError, notifySuccess, getErrorMessage } = useToasts();

const form = reactive({
  userId: typeof route.query.userId === "string" ? route.query.userId : "",
  token: typeof route.query.token === "string" ? route.query.token : "",
});

const isSubmitting = ref(false);

async function handleSubmit() {
  try {
    isSubmitting.value = true;
    await sessionStore.confirmEmail(form);
    notifySuccess("E-mail confirmado. Agora você pode entrar.");
  } catch (error) {
    notifyError(getErrorMessage(error, "Falha ao confirmar e-mail."));
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
