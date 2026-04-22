<template>
  <BaseCard title="Confirmar e-mail" subtitle="Conclui o onboarding usando o token de confirmação.">
    <form class="form" @submit.prevent="handleSubmit">
      <StatusBanner :message="statusMessage" :tone="statusTone" />
      <BaseInput v-model="form.userId" label="User Id" />
      <BaseInput v-model="form.token" label="Token" />
      <BaseButton type="submit" :loading="isSubmitting">Confirmar</BaseButton>
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
  userId: typeof route.query.userId === "string" ? route.query.userId : "",
  token: typeof route.query.token === "string" ? route.query.token : "",
});

const isSubmitting = ref(false);
const statusMessage = ref("");
const statusTone = ref<"info" | "success" | "error">("info");

async function handleSubmit() {
  try {
    isSubmitting.value = true;
    await sessionStore.confirmEmail(form);
    statusTone.value = "success";
    statusMessage.value = "E-mail confirmado. Agora voce pode entrar.";
  } catch (error) {
    statusTone.value = "error";
    statusMessage.value = error instanceof ApiClientError ? error.message : "Falha ao confirmar e-mail.";
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
