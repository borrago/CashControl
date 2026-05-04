<template>
  <Teleport to="body">
    <section class="toast-host" aria-live="polite" aria-label="Notificações">
      <article
        v-for="toast in toasts"
        :key="toast.id"
        class="toast"
        :class="`toast--${toast.tone}`"
        role="status"
      >
        <span class="toast__mark" aria-hidden="true" />
        <p>{{ toast.message }}</p>
        <button type="button" class="toast__close" aria-label="Fechar notificação" @click="removeToast(toast.id)">
          x
        </button>
      </article>
    </section>
  </Teleport>
</template>

<script setup lang="ts">
import { useToasts } from "@/shared/ui/toast.store";

const { toasts, removeToast } = useToasts();
</script>

<style scoped>
.toast-host {
  position: fixed;
  top: 1rem;
  right: 1rem;
  z-index: 1000;
  display: grid;
  width: min(380px, calc(100vw - 2rem));
  gap: 0.75rem;
  pointer-events: none;
}

.toast {
  display: grid;
  grid-template-columns: 0.45rem 1fr auto;
  gap: 0.85rem;
  align-items: center;
  min-height: 3.25rem;
  padding: 0.85rem 0.9rem 0.85rem 0;
  overflow: hidden;
  border: 1px solid var(--border);
  border-radius: 8px;
  background: #fff;
  box-shadow: var(--shadow);
  color: var(--text);
  pointer-events: auto;
}

.toast__mark {
  align-self: stretch;
}

.toast p {
  margin: 0;
  line-height: 1.35;
}

.toast__close {
  display: inline-grid;
  width: 1.8rem;
  height: 1.8rem;
  place-items: center;
  border: 0;
  border-radius: 999px;
  background: transparent;
  color: inherit;
  cursor: pointer;
}

.toast__close:hover {
  background: rgba(27, 26, 23, 0.08);
}

.toast--info .toast__mark {
  background: #2563eb;
}

.toast--success .toast__mark {
  background: #16a34a;
}

.toast--error .toast__mark {
  background: #dc2626;
}

@media (max-width: 640px) {
  .toast-host {
    top: 0.75rem;
    right: 0.75rem;
    left: 0.75rem;
    width: auto;
  }
}
</style>
