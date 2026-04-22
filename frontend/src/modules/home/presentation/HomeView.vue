<template>
  <div class="page-grid">
    <BaseCard title="Operação pronta para o backend atual" subtitle="O frontend cobre autenticação, autosserviço do usuário e administração.">
      <div class="three-columns">
        <article class="metric">
          <strong>Auth</strong>
          <span>registro, login, refresh, confirmação e reset</span>
        </article>
        <article class="metric">
          <strong>Usuário</strong>
          <span>perfil, alteração de senha e revogação de refresh token</span>
        </article>
        <article class="metric">
          <strong>Admin</strong>
          <span>busca, papéis e remoção de usuários</span>
        </article>
      </div>
    </BaseCard>

    <div class="two-columns">
      <BaseCard title="Fluxo público" subtitle="Entradas que não exigem sessão.">
        <ul class="links">
          <li><RouterLink to="/login">Entrar com credenciais</RouterLink></li>
          <li><RouterLink to="/register">Criar conta</RouterLink></li>
          <li><RouterLink to="/forgot-password">Recuperar acesso</RouterLink></li>
          <li><RouterLink to="/confirm-email">Confirmar e-mail</RouterLink></li>
          <li><RouterLink to="/reset-password">Redefinir senha</RouterLink></li>
        </ul>
      </BaseCard>

      <BaseCard title="Fluxo autenticado" subtitle="Recursos dependentes do JWT.">
        <ul class="links">
          <li><RouterLink to="/profile">Abrir painel do usuário</RouterLink></li>
          <li><RouterLink to="/admin/users">Abrir console administrativo</RouterLink></li>
        </ul>
      </BaseCard>
    </div>
  </div>
</template>

<script setup lang="ts">
import { RouterLink } from "vue-router";
import BaseCard from "@/shared/ui/BaseCard.vue";
</script>

<style scoped>
.metric {
  display: grid;
  gap: 0.4rem;
}

.metric strong {
  font-size: 1.05rem;
}

.metric span,
.links {
  color: var(--muted);
}

.links {
  margin: 0;
  padding-left: 1.2rem;
}

.links li + li {
  margin-top: 0.6rem;
}
</style>
