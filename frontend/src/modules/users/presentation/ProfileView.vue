<template>
  <div class="page-grid">
    <BaseCard title="Sessao atual" subtitle="Consulta o endpoint de usuario autenticado e mantem a sessao baseada no cookie do navegador.">
      <StatusBanner :message="statusMessage" :tone="statusTone" />

      <div v-if="sessionStore.isImpersonating" class="impersonation-banner">
        <p>
          Esta sessao esta em modo de impersonacao.
          <strong>Origem:</strong> {{ sessionStore.currentUser?.impersonatedByEmail || "sessao administrativa" }}
        </p>
        <BaseButton
          v-if="sessionStore.canStopImpersonation"
          variant="secondary"
          :loading="isStoppingImpersonation"
          @click="handleStopImpersonation"
        >
          Voltar para a sessao original
        </BaseButton>
      </div>

      <dl v-if="sessionStore.currentUser" class="details">
        <div>
          <dt>Id</dt>
          <dd>{{ sessionStore.currentUser.id }}</dd>
        </div>
        <div>
          <dt>E-mail</dt>
          <dd>{{ sessionStore.currentUser.email }}</dd>
        </div>
        <div>
          <dt>Nome</dt>
          <dd>{{ sessionStore.currentUser.fullName || "-" }}</dd>
        </div>
        <div>
          <dt>Roles</dt>
          <dd>{{ sessionStore.currentUser.roles.join(", ") || "-" }}</dd>
        </div>
        <div>
          <dt>Tenant</dt>
          <dd>{{ sessionStore.currentUser.tenant || "-" }}</dd>
        </div>
      </dl>
    </BaseCard>

    <div class="two-columns">
      <BaseCard title="Atualizar perfil">
        <form class="form" @submit.prevent="handleUpdateProfile">
          <BaseInput v-model="profileForm.fullName" label="Nome completo" />
          <BaseInput v-model="profileForm.phoneNumber" label="Telefone" />
          <BaseButton type="submit" :loading="isUpdatingProfile">Salvar perfil</BaseButton>
        </form>
      </BaseCard>

      <BaseCard title="Alterar senha">
        <form class="form" @submit.prevent="handleChangePassword">
          <BaseInput v-model="passwordForm.currentPassword" label="Senha atual" type="password" />
          <BaseInput v-model="passwordForm.newPassword" label="Nova senha" type="password" />
          <BaseButton type="submit" :loading="isChangingPassword">Atualizar senha</BaseButton>
        </form>
      </BaseCard>
    </div>

    <BaseCard title="Controle de sessao" subtitle="Encerra a sessao atual e força novo login.">
      <BaseButton variant="danger" :loading="isRevokingToken" @click="handleRevokeRefreshToken">
        Encerrar sessao
      </BaseButton>
    </BaseCard>
  </div>
</template>

<script setup lang="ts">
import { onMounted, reactive, ref, watch } from "vue";
import { useRouter } from "vue-router";
import { ApiClientError } from "@/shared/api/api-error";
import { useSessionStore } from "@/modules/auth/application/session.store";
import BaseButton from "@/shared/ui/BaseButton.vue";
import BaseCard from "@/shared/ui/BaseCard.vue";
import BaseInput from "@/shared/ui/BaseInput.vue";
import StatusBanner from "@/shared/ui/StatusBanner.vue";

const router = useRouter();
const sessionStore = useSessionStore();

const profileForm = reactive({
  fullName: "",
  phoneNumber: "",
});

const passwordForm = reactive({
  currentPassword: "",
  newPassword: "",
});

const statusMessage = ref("");
const statusTone = ref<"info" | "success" | "error">("info");
const isUpdatingProfile = ref(false);
const isChangingPassword = ref(false);
const isRevokingToken = ref(false);
const isStoppingImpersonation = ref(false);

watch(
  () => sessionStore.currentUser,
  (user) => {
    profileForm.fullName = user?.fullName ?? "";
    profileForm.phoneNumber = user?.phoneNumber ?? "";
  },
  { immediate: true },
);

onMounted(async () => {
  if (!sessionStore.currentUser) {
    await sessionStore.loadCurrentUser();
  }
});

async function handleUpdateProfile() {
  try {
    isUpdatingProfile.value = true;
    await sessionStore.updateProfile(profileForm);
    statusTone.value = "success";
    statusMessage.value = "Perfil atualizado com sucesso.";
  } catch (error) {
    statusTone.value = "error";
    statusMessage.value = error instanceof ApiClientError ? error.message : "Falha ao atualizar perfil.";
  } finally {
    isUpdatingProfile.value = false;
  }
}

async function handleChangePassword() {
  try {
    isChangingPassword.value = true;
    await sessionStore.changePassword(passwordForm);
    passwordForm.currentPassword = "";
    passwordForm.newPassword = "";
    statusTone.value = "success";
    statusMessage.value = "Senha alterada.";
  } catch (error) {
    statusTone.value = "error";
    statusMessage.value = error instanceof ApiClientError ? error.message : "Falha ao alterar senha.";
  } finally {
    isChangingPassword.value = false;
  }
}

async function handleRevokeRefreshToken() {
  try {
    isRevokingToken.value = true;
    await sessionStore.revokeRefreshToken();
    await router.push({ name: "login" });
  } catch (error) {
    statusTone.value = "error";
    statusMessage.value = error instanceof ApiClientError ? error.message : "Falha ao encerrar sessao.";
  } finally {
    isRevokingToken.value = false;
  }
}

async function handleStopImpersonation() {
  try {
    isStoppingImpersonation.value = true;
    await sessionStore.stopImpersonation();
    statusTone.value = "success";
    statusMessage.value = "Sessao original restaurada.";
  } catch (error) {
    statusTone.value = "error";
    statusMessage.value = error instanceof ApiClientError ? error.message : "Falha ao restaurar a sessao original.";
  } finally {
    isStoppingImpersonation.value = false;
  }
}
</script>

<style scoped>
.details {
  display: grid;
  gap: 0.9rem;
  grid-template-columns: repeat(auto-fit, minmax(210px, 1fr));
}

.details div,
.impersonation-banner {
  padding: 0.9rem;
  border-radius: 16px;
  background: rgba(255, 255, 255, 0.64);
}

.impersonation-banner {
  display: grid;
  gap: 0.8rem;
  margin-bottom: 1rem;
  border: 1px solid rgba(171, 92, 37, 0.2);
}

.impersonation-banner p {
  margin: 0;
}

dt {
  color: var(--muted);
  font-size: 0.88rem;
}

dd {
  margin: 0.4rem 0 0;
  font-weight: 700;
}

.form {
  display: grid;
  gap: 1rem;
}
</style>
