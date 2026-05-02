<template>
  <div class="page-grid">
    <BaseCard title="Operacao pronta para o backend atual" subtitle="O frontend cobre autenticacao por sessao, autosservico do usuario e administracao.">
      <div class="three-columns">
        <article class="metric">
          <strong>Auth</strong>
          <span>registro, login por cookie, confirmacao e reset</span>
        </article>
        <article class="metric">
          <strong>Usuario</strong>
          <span>perfil, alteracao de senha e encerramento de sessao</span>
        </article>
        <article class="metric">
          <strong>Admin</strong>
          <span>busca, papeis e remocao de usuarios</span>
        </article>
      </div>
    </BaseCard>

    <div class="two-columns">
      <BaseCard title="Fluxo publico" subtitle="Entradas que nao exigem sessao.">
        <ul class="links">
          <li><RouterLink to="/login">Entrar com credenciais</RouterLink></li>
          <li><RouterLink to="/register">Criar conta</RouterLink></li>
          <li><RouterLink to="/forgot-password">Recuperar acesso</RouterLink></li>
          <li><RouterLink to="/confirm-email">Confirmar e-mail</RouterLink></li>
          <li><RouterLink to="/reset-password">Redefinir senha</RouterLink></li>
        </ul>
      </BaseCard>

      <BaseCard title="Fluxo autenticado" subtitle="Recursos protegidos pela sessao do navegador.">
        <ul class="links">
          <li><RouterLink to="/profile">Abrir painel do usuario</RouterLink></li>
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
