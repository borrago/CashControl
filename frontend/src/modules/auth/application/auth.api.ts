import { httpClient } from "@/shared/api/http-client";
import type {
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
    return httpClient.post<void>("/auth/login", payload);
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
