import { computed, ref } from "vue";
import { defineStore } from "pinia";
import { authApi } from "@/modules/auth/application/auth.api";
import { usersApi } from "@/modules/users/application/users.api";
import type {
  AuthTokens,
  ConfirmEmailPayload,
  ForgotPasswordPayload,
  LoginPayload,
  RegisterPayload,
  ResetPasswordPayload,
} from "@/modules/auth/domain/auth.types";
import type { ChangePasswordPayload, UpdateProfilePayload, UserProfile } from "@/modules/users/domain/user.types";
import { sessionStorageService } from "@/shared/storage/session-storage";

export const useSessionStore = defineStore("session", () => {
  const accessToken = ref<string | null>(null);
  const refreshToken = ref<string | null>(null);
  const refreshTokenExpiresAtUtc = ref<string | null>(null);
  const currentUser = ref<UserProfile | null>(null);
  const isBootstrapped = ref(false);
  const isRefreshing = ref(false);

  const isAuthenticated = computed(() => Boolean(accessToken.value && refreshToken.value));
  const isAdmin = computed(() => currentUser.value?.roles.includes("Admin") ?? false);

  function persistTokens(tokens: AuthTokens) {
    accessToken.value = tokens.accessToken;
    refreshToken.value = tokens.refreshToken;
    refreshTokenExpiresAtUtc.value = tokens.refreshTokenExpiresAtUtc;
    sessionStorageService.write(tokens);
  }

  function clearSession() {
    accessToken.value = null;
    refreshToken.value = null;
    refreshTokenExpiresAtUtc.value = null;
    currentUser.value = null;
    sessionStorageService.clear();
  }

  async function bootstrap() {
    if (isBootstrapped.value) {
      return;
    }

    const stored = sessionStorageService.read();

    if (stored) {
      accessToken.value = stored.accessToken;
      refreshToken.value = stored.refreshToken;
      refreshTokenExpiresAtUtc.value = stored.refreshTokenExpiresAtUtc;

      try {
        await loadCurrentUser();
      } catch {
        clearSession();
      }
    }

    isBootstrapped.value = true;
  }

  async function loadCurrentUser() {
    currentUser.value = await usersApi.getCurrentUser();
    return currentUser.value;
  }

  async function register(payload: RegisterPayload) {
    await authApi.register(payload);
  }

  async function login(payload: LoginPayload) {
    const tokens = await authApi.login(payload);
    persistTokens(tokens);
    await loadCurrentUser();
  }

  async function refreshSession(): Promise<boolean> {
    if (isRefreshing.value || !accessToken.value || !refreshToken.value || !refreshTokenExpiresAtUtc.value) {
      return false;
    }

    try {
      isRefreshing.value = true;

      const tokens = await authApi.refreshToken({
        accessToken: accessToken.value,
        refreshToken: refreshToken.value,
        refreshTokenExpiresAtUtc: refreshTokenExpiresAtUtc.value,
      });

      persistTokens(tokens);
      return true;
    } catch {
      clearSession();
      return false;
    } finally {
      isRefreshing.value = false;
    }
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

  function logout(options: { redirect?: boolean } = {}) {
    void options;
    clearSession();
  }

  return {
    accessToken,
    refreshToken,
    refreshTokenExpiresAtUtc,
    currentUser,
    isBootstrapped,
    isAuthenticated,
    isAdmin,
    bootstrap,
    register,
    login,
    refreshSession,
    loadCurrentUser,
    forgotPassword,
    resetPassword,
    confirmEmail,
    updateProfile,
    changePassword,
    revokeRefreshToken,
    logout,
  };
});
