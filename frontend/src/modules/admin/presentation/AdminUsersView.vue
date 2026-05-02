<template>
  <div class="page-grid">
    <BaseCard title="Busca administrativa" subtitle="Consulta usuario e papeis usando os endpoints de administracao.">
      <form class="search" @submit.prevent="handleFetchUser">
        <BaseInput v-model="selectedUserId" label="User Id" placeholder="Informe o id do usuario" />
        <BaseButton type="submit" :loading="isFetching">Buscar</BaseButton>
      </form>
      <StatusBanner :message="statusMessage" :tone="statusTone" />
    </BaseCard>

    <div class="two-columns">
      <BaseCard title="Usuario">
        <dl v-if="user" class="details">
          <div><dt>Nome</dt><dd>{{ user.fullName || "-" }}</dd></div>
          <div><dt>E-mail</dt><dd>{{ user.email }}</dd></div>
          <div><dt>Username</dt><dd>{{ user.userName || "-" }}</dd></div>
          <div><dt>Telefone</dt><dd>{{ user.phoneNumber || "-" }}</dd></div>
          <div><dt>Tenant</dt><dd>{{ user.tenant || "-" }}</dd></div>
        </dl>
        <p v-else class="empty">Nenhum usuario carregado.</p>
      </BaseCard>

      <BaseCard title="Papeis">
        <ul v-if="roles.length" class="roles">
          <li v-for="role in roles" :key="role">{{ role }}</li>
        </ul>
        <p v-else class="empty">Nenhum papel carregado.</p>
      </BaseCard>
    </div>

    <div class="two-columns">
      <BaseCard title="Atribuir papel">
        <form class="form" @submit.prevent="handleAssignRole">
          <BaseInput v-model="assignRole" label="Papel" placeholder="Admin, Manager..." />
          <BaseButton type="submit" :loading="isAssigning">Atribuir</BaseButton>
        </form>
      </BaseCard>

      <BaseCard title="Remover papel">
        <form class="form" @submit.prevent="handleRemoveRole">
          <BaseInput v-model="removeRole" label="Papel" placeholder="Papel a remover" />
          <BaseButton type="submit" variant="secondary" :loading="isRemoving">Remover</BaseButton>
        </form>
      </BaseCard>
    </div>

    <BaseCard title="Acoes sensiveis" subtitle="Impersonacao disponivel apenas para sessoes com permissao global.">
      <div class="actions">
        <BaseButton
          v-if="sessionStore.currentUser?.canImpersonateUsers"
          variant="secondary"
          :disabled="!selectedUserId"
          :loading="isImpersonating"
          @click="handleImpersonate"
        >
          Impersonar usuario
        </BaseButton>
        <BaseButton variant="danger" :disabled="!selectedUserId" :loading="isDeleting" @click="handleDelete">
          Excluir usuario
        </BaseButton>
      </div>
    </BaseCard>
  </div>
</template>

<script setup lang="ts">
import { ref } from "vue";
import { useRouter } from "vue-router";
import { ApiClientError } from "@/shared/api/api-error";
import { useSessionStore } from "@/modules/auth/application/session.store";
import { adminUsersApi } from "@/modules/admin/application/admin-users.api";
import type { AdminUser } from "@/modules/admin/domain/admin.types";
import { clearCsrfTokenCache } from "@/shared/api/http-client";
import BaseButton from "@/shared/ui/BaseButton.vue";
import BaseCard from "@/shared/ui/BaseCard.vue";
import BaseInput from "@/shared/ui/BaseInput.vue";
import StatusBanner from "@/shared/ui/StatusBanner.vue";

const router = useRouter();
const sessionStore = useSessionStore();
const selectedUserId = ref("");
const assignRole = ref("");
const removeRole = ref("");
const user = ref<AdminUser | null>(null);
const roles = ref<string[]>([]);
const statusMessage = ref("");
const statusTone = ref<"info" | "success" | "error">("info");
const isFetching = ref(false);
const isAssigning = ref(false);
const isRemoving = ref(false);
const isDeleting = ref(false);
const isImpersonating = ref(false);

async function refreshRoles() {
  if (!selectedUserId.value) {
    return;
  }

  const response = await adminUsersApi.getRoles(selectedUserId.value);
  roles.value = response.roles;
}

async function handleFetchUser() {
  try {
    isFetching.value = true;
    user.value = await adminUsersApi.getById(selectedUserId.value);
    await refreshRoles();
    statusTone.value = "success";
    statusMessage.value = "Usuario carregado.";
  } catch (error) {
    statusTone.value = "error";
    statusMessage.value = error instanceof ApiClientError ? error.message : "Falha ao buscar usuario.";
  } finally {
    isFetching.value = false;
  }
}

async function handleAssignRole() {
  try {
    isAssigning.value = true;
    await adminUsersApi.assignRole(selectedUserId.value, assignRole.value);
    assignRole.value = "";
    await refreshRoles();
    statusTone.value = "success";
    statusMessage.value = "Papel atribuido.";
  } catch (error) {
    statusTone.value = "error";
    statusMessage.value = error instanceof ApiClientError ? error.message : "Falha ao atribuir papel.";
  } finally {
    isAssigning.value = false;
  }
}

async function handleRemoveRole() {
  try {
    isRemoving.value = true;
    await adminUsersApi.removeRole(selectedUserId.value, removeRole.value);
    removeRole.value = "";
    await refreshRoles();
    statusTone.value = "success";
    statusMessage.value = "Papel removido.";
  } catch (error) {
    statusTone.value = "error";
    statusMessage.value = error instanceof ApiClientError ? error.message : "Falha ao remover papel.";
  } finally {
    isRemoving.value = false;
  }
}

async function handleDelete() {
  try {
    isDeleting.value = true;
    await adminUsersApi.deleteUser(selectedUserId.value);
    user.value = null;
    roles.value = [];
    statusTone.value = "success";
    statusMessage.value = "Usuario excluido.";
  } catch (error) {
    statusTone.value = "error";
    statusMessage.value = error instanceof ApiClientError ? error.message : "Falha ao excluir usuario.";
  } finally {
    isDeleting.value = false;
  }
}

async function handleImpersonate() {
  try {
    isImpersonating.value = true;
    await adminUsersApi.impersonate(selectedUserId.value);
    clearCsrfTokenCache();
    await sessionStore.loadCurrentUser();
    statusTone.value = "success";
    statusMessage.value = "Sessao trocada para o usuario selecionado.";
    await router.push({ name: "profile" });
  } catch (error) {
    statusTone.value = "error";
    statusMessage.value = error instanceof ApiClientError ? error.message : "Falha ao iniciar impersonacao.";
  } finally {
    isImpersonating.value = false;
  }
}
</script>

<style scoped>
.search,
.form {
  display: grid;
  gap: 1rem;
}

.details {
  display: grid;
  gap: 0.9rem;
}

dt {
  color: var(--muted);
  font-size: 0.88rem;
}

dd {
  margin: 0.35rem 0 0;
  font-weight: 700;
}

.roles {
  margin: 0;
  padding-left: 1.1rem;
}

.actions {
  display: flex;
  gap: 1rem;
  flex-wrap: wrap;
}

.empty {
  margin: 0;
  color: var(--muted);
}
</style>
