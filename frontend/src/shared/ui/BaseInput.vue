<template>
  <label class="field">
    <span class="field__label">{{ label }}</span>
    <input
      class="field__input"
      :type="type"
      :value="modelValue"
      :placeholder="placeholder"
      @input="$emit('update:modelValue', ($event.target as HTMLInputElement).value)"
    />
  </label>
</template>

<script setup lang="ts">
withDefaults(
  defineProps<{
    label: string;
    modelValue: string;
    type?: string;
    placeholder?: string;
  }>(),
  {
    type: "text",
    placeholder: "",
  },
);

defineEmits<{
  "update:modelValue": [value: string];
}>();
</script>

<style scoped>
.field {
  display: grid;
  gap: 0.45rem;
}

.field__label {
  font-size: 0.92rem;
  color: var(--muted);
}

.field__input {
  width: 100%;
  padding: 0.9rem 1rem;
  border: 1px solid var(--border);
  border-radius: 16px;
  background: rgba(255, 255, 255, 0.74);
}

.field__input:focus {
  outline: 2px solid rgba(15, 118, 110, 0.25);
  border-color: var(--primary);
}
</style>
