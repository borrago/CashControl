import { httpClient } from "@/shared/api/http-client";
import type { AdminUser, UserRolesResponse } from "@/modules/admin/domain/admin.types";

export const adminUsersApi = {
  getById(userId: string) {
    return httpClient.get<AdminUser>(`/admin/users/${userId}`, { requiresAuth: true });
  },
  getRoles(userId: string) {
    return httpClient.get<UserRolesResponse>(`/admin/users/${userId}/roles`, { requiresAuth: true });
  },
  impersonate(userId: string) {
    return httpClient.post<void>(`/admin/users/${userId}/impersonate`, undefined, { requiresAuth: true });
  },
  assignRole(userId: string, role: string) {
    return httpClient.put<void>(`/admin/users/${userId}/roles/${role}`, undefined, { requiresAuth: true });
  },
  removeRole(userId: string, role: string) {
    return httpClient.delete<void>(`/admin/users/${userId}/roles/${role}`, { requiresAuth: true });
  },
  deleteUser(userId: string) {
    return httpClient.delete<void>(`/admin/users/${userId}`, { requiresAuth: true });
  },
};
