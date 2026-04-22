<template>
  <section class="card">
    <header v-if="title || subtitle" class="card__header">
      <div>
        <h2 v-if="title">{{ title }}</h2>
        <p v-if="subtitle">{{ subtitle }}</p>
      </div>
      <slot name="header" />
    </header>
    <slot />
  </section>
</template>

<script setup lang="ts">
defineProps<{
  title?: string;
  subtitle?: string;
}>();
</script>

<style scoped>
.card {
  padding: 1.25rem;
  border: 1px solid var(--border);
  border-radius: 24px;
  background: var(--bg-panel);
  backdrop-filter: blur(16px);
  box-shadow: var(--shadow);
}

.card__header {
  display: flex;
  justify-content: space-between;
  gap: 1rem;
  align-items: flex-start;
  margin-bottom: 1rem;
}

h2 {
  margin: 0;
  font-size: 1.2rem;
}

p {
  margin: 0.4rem 0 0;
  color: var(--muted);
}
</style>
