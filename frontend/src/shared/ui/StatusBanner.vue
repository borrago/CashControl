<template>
  <p v-if="message" class="banner" :class="`banner--${tone}`">
    {{ message }}
  </p>
</template>

<script setup lang="ts">
withDefaults(
  defineProps<{
    message?: string;
    tone?: "info" | "success" | "error";
  }>(),
  {
    message: "",
    tone: "info",
  },
);
</script>

<style scoped>
.banner {
  margin: 0;
  padding: 0.85rem 1rem;
  border-radius: 14px;
  border: 1px solid transparent;
}

.banner--info {
  background: rgba(15, 118, 110, 0.08);
  border-color: rgba(15, 118, 110, 0.16);
}

.banner--success {
  background: rgba(34, 197, 94, 0.08);
  border-color: rgba(34, 197, 94, 0.16);
}

.banner--error {
  background: rgba(185, 28, 28, 0.08);
  border-color: rgba(185, 28, 28, 0.16);
  color: var(--danger);
}
</style>
