<template>
  <div class="page-grid">
    <BaseCard title="Busca administrativa" subtitle="Consulta usuário e papéis usando os endpoints de administração.">
      <form class="search" @submit.prevent="handleFetchUser">
        <BaseInput v-model="selectedUserId" label="User Id" placeholder="Informe o id do usuário" />
        <BaseButton type="submit" :loading="isFetching">Buscar</BaseButton>
      </form>
    </BaseCard>

    <div class="two-columns">
      <BaseCard title="Usuário">
        <dl v-if="user" class="details">
          <div><dt>Nome</dt><dd>{{ user.fullName || "-" }}</dd></div>
          <div><dt>E-mail</dt><dd>{{ user.email }}</dd></div>
          <div><dt>Username</dt><dd>{{ user.userName || "-" }}</dd></div>
          <div><dt>Telefone</dt><dd>{{ user.phoneNumber || "-" }}</dd></div>
          <div><dt>Tenant</dt><dd>{{ user.tenant || "-" }}</dd></div>
        </dl>
        <p v-else class="empty">Nenhum usuário carregado.</p>
      </BaseCard>

      <BaseCard title="Papéis">
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

    <BaseCard title="Ações sensíveis" subtitle="Impersonação disponível apenas para sessões com permissão global.">
      <div class="actions">
        <BaseButton
          v-if="sessionStore.currentUser?.canImpersonateUsers"
          variant="secondary"
          :disabled="!selectedUserId"
          :loading="isImpersonating"
          @click="handleImpersonate"
        >
          Impersonar usuário
        </BaseButton>
        <BaseButton variant="danger" :disabled="!selectedUserId" :loading="isDeleting" @click="handleDelete">
          Excluir usuário
        </BaseButton>
      </div>
    </BaseCard>
  </div>
</template>

<script setup lang="ts">
import { ref } from "vue";
import { useRouter } from "vue-router";
import { useSessionStore } from "@/modules/auth/application/session.store";
import { adminUsersApi } from "@/modules/admin/application/admin-users.api";
import type { AdminUser } from "@/modules/admin/domain/admin.types";
import { clearCsrfTokenCache } from "@/shared/api/http-client";
import BaseButton from "@/shared/ui/BaseButton.vue";
import BaseCard from "@/shared/ui/BaseCard.vue";
import BaseInput from "@/shared/ui/BaseInput.vue";
import { useToasts } from "@/shared/ui/toast.store";

const router = useRouter();
const sessionStore = useSessionStore();
const { notifyError, notifySuccess, getErrorMessage } = useToasts();
const selectedUserId = ref("");
const assignRole = ref("");
const removeRole = ref("");
const user = ref<AdminUser | null>(null);
const roles = ref<string[]>([]);
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
    notifySuccess("Usuário carregado.");
  } catch (error) {
    notifyError(getErrorMessage(error, "Falha ao buscar usuário."));
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
    notifySuccess("Papel atribuído.");
  } catch (error) {
    notifyError(getErrorMessage(error, "Falha ao atribuir papel."));
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
    notifySuccess("Papel removido.");
  } catch (error) {
    notifyError(getErrorMessage(error, "Falha ao remover papel."));
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
    notifySuccess("Usuário excluído.");
  } catch (error) {
    notifyError(getErrorMessage(error, "Falha ao excluir usuário."));
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
    notifySuccess("Sessão trocada para o usuário selecionado.");
    await router.push({ name: "profile" });
  } catch (error) {
    notifyError(getErrorMessage(error, "Falha ao iniciar impersonação."));
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
