import type { UserProfile } from "@/modules/users/domain/user.types";

export interface UserRolesResponse {
  userId: string;
  roles: string[];
}

export interface AssignRolePayload {
  userId: string;
  role: string;
}

export interface RemoveRolePayload {
  userId: string;
  role: string;
}

export type AdminUser = UserProfile;
