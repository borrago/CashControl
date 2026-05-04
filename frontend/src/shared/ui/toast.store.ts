import { readonly, ref } from "vue";
import { ApiClientError } from "@/shared/api/api-error";

export type ToastTone = "info" | "success" | "error";

export interface ToastMessage {
  id: number;
  message: string;
  tone: ToastTone;
}

const toasts = ref<ToastMessage[]>([]);
let nextId = 1;

function removeToast(id: number) {
  toasts.value = toasts.value.filter((toast) => toast.id !== id);
}

function notify(message: string, tone: ToastTone = "info") {
  const id = nextId++;
  toasts.value = [...toasts.value, { id, message, tone }];
  window.setTimeout(() => removeToast(id), 4200);
}

function getErrorMessage(error: unknown, fallback: string) {
  if (error instanceof ApiClientError) {
    return error.response?.errors?.find((detail) => detail.errorMessage)?.errorMessage
      ?? error.message
      ?? fallback;
  }

  return fallback;
}

export function useToasts() {
  return {
    toasts: readonly(toasts),
    notify,
    notifyInfo: (message: string) => notify(message, "info"),
    notifySuccess: (message: string) => notify(message, "success"),
    notifyError: (message: string) => notify(message, "error"),
    removeToast,
    getErrorMessage,
  };
}
