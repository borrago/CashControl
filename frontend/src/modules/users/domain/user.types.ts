export interface UserProfile {
  id: string;
  email: string;
  userName?: string | null;
  fullName?: string | null;
  phoneNumber?: string | null;
  tenant?: number;
  canImpersonateUsers?: boolean;
  isImpersonating?: boolean;
  canStopImpersonation?: boolean;
  impersonatedByEmail?: string | null;
  roles: string[];
}

export interface UpdateProfilePayload {
  fullName?: string;
  phoneNumber?: string;
}

export interface ChangePasswordPayload {
  currentPassword: string;
  newPassword: string;
}
