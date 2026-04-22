<template>
  <button class="button" :class="[`button--${variant}`]" :type="type" :disabled="disabled || loading">
    <span v-if="loading">Processando...</span>
    <span v-else><slot /></span>
  </button>
</template>

<script setup lang="ts">
withDefaults(
  defineProps<{
    variant?: "primary" | "secondary" | "danger";
    type?: "button" | "submit";
    disabled?: boolean;
    loading?: boolean;
  }>(),
  {
    variant: "primary",
    type: "button",
    disabled: false,
    loading: false,
  },
);
</script>

<style scoped>
.button {
  border: none;
  border-radius: 999px;
  padding: 0.82rem 1.1rem;
  cursor: pointer;
  transition: transform 0.2s ease, opacity 0.2s ease, background-color 0.2s ease;
  font-weight: 700;
}

.button:hover:not(:disabled) {
  transform: translateY(-1px);
}

.button:disabled {
  opacity: 0.65;
  cursor: not-allowed;
}

.button--primary {
  background: var(--primary);
  color: white;
}

.button--secondary {
  background: rgba(15, 118, 110, 0.1);
  color: var(--primary-strong);
}

.button--danger {
  background: rgba(185, 28, 28, 0.12);
  color: var(--danger);
}
</style>
