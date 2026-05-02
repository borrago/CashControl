import { afterEach, describe, expect, it, vi } from "vitest";
import { configureHttpClient, httpClient } from "@/shared/api/http-client";

describe("httpClient", () => {
  afterEach(() => {
    vi.restoreAllMocks();
    configureHttpClient({
      clearSession: () => undefined,
    });
  });

  it("envia credenciais e limpa a sessao quando recebe 401 em rota autenticada", async () => {
    const clearSession = vi.fn();
    const fetchMock = vi.spyOn(globalThis, "fetch").mockResolvedValue(
      new Response(
        JSON.stringify({
          code: "unauthorized",
          message: "expirado",
          errors: [],
        }),
        { status: 401, headers: { "content-type": "application/json" } },
      ),
    );

    configureHttpClient({ clearSession });

    await expect(httpClient.get("/users/me", { requiresAuth: true })).rejects.toMatchObject({ statusCode: 401 });

    expect(fetchMock).toHaveBeenCalledWith(
      expect.any(String),
      expect.objectContaining({ credentials: "include" }),
    );
    expect(clearSession).toHaveBeenCalledTimes(1);
  });
});
