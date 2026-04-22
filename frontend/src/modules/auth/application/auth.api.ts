import { httpClient } from "@/shared/api/http-client";
import type {
  AuthTokens,
  ConfirmEmailPayload,
  ForgotPasswordPayload,
  LoginPayload,
  RegisterPayload,
  ResetPasswordPayload,
} from "@/modules/auth/domain/auth.types";

export const authApi = {
  register(payload: RegisterPayload) {
    return httpClient.post<void>("/auth/register", payload);
  },
  login(payload: LoginPayload) {
    return httpClient.post<AuthTokens>("/auth/login", payload);
  },
  refreshToken(payload: AuthTokens) {
    return httpClient.post<AuthTokens>("/auth/refresh-token", {
      accessToken: payload.accessToken,
      refreshToken: payload.refreshToken,
    });
  },
  forgotPassword(payload: ForgotPasswordPayload) {
    return httpClient.post<void>("/auth/forgot-password", payload);
  },
  resetPassword(payload: ResetPasswordPayload) {
    return httpClient.post<void>("/auth/reset-password", payload);
  },
  confirmEmail(payload: ConfirmEmailPayload) {
    return httpClient.post<void>("/auth/confirm-email", payload);
  },
};
