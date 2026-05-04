import { beforeEach, describe, expect, it, vi } from "vitest";
import { createPinia, setActivePinia } from "pinia";
import { useSessionStore } from "@/modules/auth/application/session.store";
import { authApi } from "@/modules/auth/application/auth.api";
import { usersApi } from "@/modules/users/application/users.api";

vi.mock("@/modules/auth/application/auth.api", () => ({
  authApi: {
    register: vi.fn(),
    login: vi.fn(),
    forgotPassword: vi.fn(),
    resetPassword: vi.fn(),
    confirmEmail: vi.fn(),
  },
}));

vi.mock("@/modules/users/application/users.api", () => ({
  usersApi: {
    getCurrentUser: vi.fn(),
    updateProfile: vi.fn(),
    changePassword: vi.fn(),
    revokeRefreshToken: vi.fn(),
    stopImpersonation: vi.fn(),
  },
}));

describe("session.store", () => {
  beforeEach(() => {
    setActivePinia(createPinia());
  });

  it("faz login via cookie e carrega o usuário atual", async () => {
    vi.mocked(authApi.login).mockResolvedValue();

    vi.mocked(usersApi.getCurrentUser).mockResolvedValue({
      id: "1",
      email: "john@cashcontrol.com",
      fullName: "John",
      phoneNumber: null,
      userName: "john",
      roles: ["Admin"],
    });

    const store = useSessionStore();

    await store.login({
      email: "john@cashcontrol.com",
      password: "Pass123",
    });

    expect(store.isAuthenticated).toBe(true);
    expect(store.isAdmin).toBe(true);
    expect(store.currentUser?.email).toBe("john@cashcontrol.com");
    expect(authApi.login).toHaveBeenCalledTimes(1);
  });
});
