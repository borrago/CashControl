<template>
  <div class="shell">
    <header class="shell__header">
      <div class="container shell__header-inner">
        <div>
          <p class="shell__eyebrow">CashControl</p>
          <h1>Identity Console</h1>
        </div>

        <nav class="shell__nav">
          <RouterLink to="/">Início</RouterLink>
          <RouterLink v-if="!sessionStore.isAuthenticated" to="/login">Login</RouterLink>
          <RouterLink v-if="!sessionStore.isAuthenticated" to="/register">Cadastro</RouterLink>
          <RouterLink v-if="sessionStore.isAuthenticated" to="/profile">Perfil</RouterLink>
          <RouterLink v-if="sessionStore.isAdmin" to="/admin/users">Admin</RouterLink>
          <BaseButton v-if="sessionStore.isAuthenticated" variant="secondary" @click="logout">
            Sair
          </BaseButton>
        </nav>
      </div>
    </header>

    <main class="container shell__main">
      <slot />
    </main>
  </div>
</template>

<script setup lang="ts">
import { RouterLink, useRouter } from "vue-router";
import { useSessionStore } from "@/modules/auth/application/session.store";
import BaseButton from "@/shared/ui/BaseButton.vue";

const router = useRouter();
const sessionStore = useSessionStore();

function logout() {
  sessionStore.logout();
  void router.push({ name: "login" });
}
</script>

<style scoped>
.shell {
  min-height: 100vh;
}

.shell__header {
  position: sticky;
  top: 0;
  z-index: 10;
  backdrop-filter: blur(12px);
  background: rgba(246, 241, 232, 0.75);
  border-bottom: 1px solid rgba(27, 26, 23, 0.08);
}

.shell__header-inner {
  display: flex;
  justify-content: space-between;
  gap: 1rem;
  align-items: center;
  padding: 1rem 0;
}

.shell__eyebrow {
  margin: 0;
  text-transform: uppercase;
  font-size: 0.72rem;
  letter-spacing: 0.18em;
  color: var(--accent);
}

h1 {
  margin: 0.2rem 0 0;
  font-size: clamp(1.3rem, 3vw, 2.4rem);
}

.shell__nav {
  display: flex;
  gap: 0.8rem;
  align-items: center;
  flex-wrap: wrap;
}

.shell__nav a.router-link-active {
  color: var(--primary);
  font-weight: 700;
}

.shell__main {
  padding: 2rem 0 3rem;
}

@media (max-width: 760px) {
  .shell__header-inner {
    align-items: flex-start;
    flex-direction: column;
  }
}
</style>
