import { describe, expect, it, vi } from "vitest";
import { render, screen } from "@testing-library/vue";
import userEvent from "@testing-library/user-event";
import { createPinia } from "pinia";
import AdminUsersView from "@/modules/admin/presentation/AdminUsersView.vue";
import { adminUsersApi } from "@/modules/admin/application/admin-users.api";

vi.mock("@/modules/admin/application/admin-users.api", () => ({
  adminUsersApi: {
    getById: vi.fn(),
    getRoles: vi.fn(),
    assignRole: vi.fn(),
    removeRole: vi.fn(),
    deleteUser: vi.fn(),
  },
}));

describe("AdminUsersView", () => {
  it("consulta usuário e renderiza os papéis carregados", async () => {
    vi.mocked(adminUsersApi.getById).mockResolvedValue({
      id: "user-1",
      email: "admin@cashcontrol.com",
      fullName: "Admin User",
      phoneNumber: "5511999999999",
      userName: "admin",
      roles: ["Admin"],
    });

    vi.mocked(adminUsersApi.getRoles).mockResolvedValue({
      userId: "user-1",
      roles: ["Admin", "Manager"],
    });

    render(AdminUsersView, {
      global: {
        plugins: [createPinia()],
      },
    });

    await userEvent.type(screen.getByLabelText("User Id"), "user-1");
    await userEvent.click(screen.getByRole("button", { name: "Buscar" }));

    expect(await screen.findByText("Admin User")).toBeInTheDocument();
    expect(await screen.findByText("Manager")).toBeInTheDocument();
  });
});
