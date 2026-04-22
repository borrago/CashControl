import { afterEach, describe, expect, it, vi } from "vitest";
import { configureHttpClient, httpClient } from "@/shared/api/http-client";

describe("httpClient", () => {
  afterEach(() => {
    vi.restoreAllMocks();
    configureHttpClient({
      getAccessToken: () => null,
      refreshSession: async () => false,
      clearSession: () => undefined,
    });
  });

  it("renova a sessão e repete a requisição autenticada quando recebe 401", async () => {
    const fetchMock = vi
      .spyOn(globalThis, "fetch")
      .mockResolvedValueOnce(
        new Response(
          JSON.stringify({
            code: "unauthorized",
            message: "expirado",
            errors: [],
          }),
          { status: 401, headers: { "content-type": "application/json" } },
        ),
      )
      .mockResolvedValueOnce(
        new Response(JSON.stringify({ id: "1", email: "john@cashcontrol.com", roles: [] }), {
          status: 200,
          headers: { "content-type": "application/json" },
        }),
      );

    const refreshSession = vi.fn().mockResolvedValue(true);

    configureHttpClient({
      getAccessToken: () => "token-antigo",
      refreshSession,
      clearSession: vi.fn(),
    });

    const response = await httpClient.get<{ id: string; email: string; roles: string[] }>("/users/me", {
      requiresAuth: true,
    });

    expect(refreshSession).toHaveBeenCalledTimes(1);
    expect(fetchMock).toHaveBeenCalledTimes(2);
    expect(response.email).toBe("john@cashcontrol.com");
  });
});
