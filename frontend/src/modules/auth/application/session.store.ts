import { computed, ref } from "vue";
import { defineStore } from "pinia";
import { authApi } from "@/modules/auth/application/auth.api";
import { usersApi } from "@/modules/users/application/users.api";
import { clearCsrfTokenCache } from "@/shared/api/http-client";
import type {
  ConfirmEmailPayload,
  ForgotPasswordPayload,
  LoginPayload,
  RegisterPayload,
  ResetPasswordPayload,
} from "@/modules/auth/domain/auth.types";
import type { ChangePasswordPayload, UpdateProfilePayload, UserProfile } from "@/modules/users/domain/user.types";

export const useSessionStore = defineStore("session", () => {
  const currentUser = ref<UserProfile | null>(null);
  const isBootstrapped = ref(false);

  const isAuthenticated = computed(() => currentUser.value !== null);
  const isAdmin = computed(() => currentUser.value?.roles.includes("Admin") ?? false);
  const isImpersonating = computed(() => currentUser.value?.isImpersonating ?? false);
  const canStopImpersonation = computed(() => currentUser.value?.canStopImpersonation ?? false);

  function clearSession() {
    currentUser.value = null;
    clearCsrfTokenCache();
  }

  async function bootstrap() {
    if (isBootstrapped.value) {
      return;
    }

    try {
      await loadCurrentUser();
    } catch {
      clearSession();
    } finally {
      isBootstrapped.value = true;
    }
  }

  async function loadCurrentUser() {
    currentUser.value = await usersApi.getCurrentUser();
    return currentUser.value;
  }

  async function register(payload: RegisterPayload) {
    await authApi.register(payload);
  }

  async function login(payload: LoginPayload) {
    await authApi.login(payload);
    clearCsrfTokenCache();
    await loadCurrentUser();
  }

  async function forgotPassword(payload: ForgotPasswordPayload) {
    await authApi.forgotPassword(payload);
  }

  async function resetPassword(payload: ResetPasswordPayload) {
    await authApi.resetPassword(payload);
  }

  async function confirmEmail(payload: ConfirmEmailPayload) {
    await authApi.confirmEmail(payload);
  }

  async function updateProfile(payload: UpdateProfilePayload) {
    await usersApi.updateProfile(payload);
    await loadCurrentUser();
  }

  async function changePassword(payload: ChangePasswordPayload) {
    await usersApi.changePassword(payload);
  }

  async function revokeRefreshToken() {
    await usersApi.revokeRefreshToken();
    clearSession();
  }

  async function stopImpersonation() {
    await usersApi.stopImpersonation();
    clearCsrfTokenCache();
    await loadCurrentUser();
  }

  async function logout(options: { redirect?: boolean } = {}) {
    void options;

    if (currentUser.value) {
      try {
        await usersApi.revokeRefreshToken();
      } catch {
        // Keep local state consistent even if the session already expired.
      }
    }

    clearSession();
  }

  return {
    currentUser,
    isBootstrapped,
    isAuthenticated,
    isAdmin,
    isImpersonating,
    canStopImpersonation,
    clearSession,
    bootstrap,
    register,
    login,
    loadCurrentUser,
    forgotPassword,
    resetPassword,
    confirmEmail,
    updateProfile,
    changePassword,
    revokeRefreshToken,
    stopImpersonation,
    logout,
  };
});
