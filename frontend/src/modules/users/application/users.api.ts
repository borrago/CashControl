import { httpClient } from "@/shared/api/http-client";
import type { ChangePasswordPayload, UpdateProfilePayload, UserProfile } from "@/modules/users/domain/user.types";

export const usersApi = {
  getCurrentUser() {
    return httpClient.get<UserProfile>("/users/me", { requiresAuth: true });
  },
  updateProfile(payload: UpdateProfilePayload) {
    return httpClient.put<void>("/users/me", payload, { requiresAuth: true });
  },
  changePassword(payload: ChangePasswordPayload) {
    return httpClient.post<void>("/users/me/change-password", payload, { requiresAuth: true });
  },
  revokeRefreshToken() {
    return httpClient.delete<void>("/users/me/refresh-token", { requiresAuth: true });
  },
};
