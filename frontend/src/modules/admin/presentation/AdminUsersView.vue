<template>
  <div class="page-grid">
    <BaseCard title="Busca administrativa" subtitle="Consulta usuário e papéis usando os endpoints de administração.">
      <form class="search" @submit.prevent="handleFetchUser">
        <BaseInput v-model="selectedUserId" label="User Id" placeholder="Informe o id do usuário" />
        <BaseButton type="submit" :loading="isFetching">Buscar</BaseButton>
      </form>
      <StatusBanner :message="statusMessage" :tone="statusTone" />
    </BaseCard>

    <div class="two-columns">
      <BaseCard title="Usuário">
        <dl v-if="user" class="details">
          <div><dt>Nome</dt><dd>{{ user.fullName || "-" }}</dd></div>
          <div><dt>E-mail</dt><dd>{{ user.email }}</dd></div>
          <div><dt>Username</dt><dd>{{ user.userName || "-" }}</dd></div>
          <div><dt>Telefone</dt><dd>{{ user.phoneNumber || "-" }}</dd></div>
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

    <BaseCard title="Exclusão" subtitle="Remove definitivamente o usuário informado.">
      <BaseButton variant="danger" :disabled="!selectedUserId" :loading="isDeleting" @click="handleDelete">
        Excluir usuário
      </BaseButton>
    </BaseCard>
  </div>
</template>

<script setup lang="ts">
import { ref } from "vue";
import { ApiClientError } from "@/shared/api/api-error";
import { adminUsersApi } from "@/modules/admin/application/admin-users.api";
import type { AdminUser } from "@/modules/admin/domain/admin.types";
import BaseButton from "@/shared/ui/BaseButton.vue";
import BaseCard from "@/shared/ui/BaseCard.vue";
import BaseInput from "@/shared/ui/BaseInput.vue";
import StatusBanner from "@/shared/ui/StatusBanner.vue";

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
    statusMessage.value = "Usuário carregado.";
  } catch (error) {
    statusTone.value = "error";
    statusMessage.value = error instanceof ApiClientError ? error.message : "Falha ao buscar usuário.";
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
    statusMessage.value = "Papel atribuído.";
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
    statusMessage.value = "Usuário excluído.";
  } catch (error) {
    statusTone.value = "error";
    statusMessage.value = error instanceof ApiClientError ? error.message : "Falha ao excluir usuário.";
  } finally {
    isDeleting.value = false;
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

.empty {
  margin: 0;
  color: var(--muted);
}
</style>
