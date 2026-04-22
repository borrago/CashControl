import { env } from "@/shared/config/env";
import { ApiClientError, parseApiError } from "@/shared/api/api-error";

type Primitive = string | number | boolean | null | undefined;
type QueryValue = Primitive | Primitive[];

interface RequestOptions {
  method?: "GET" | "POST" | "PUT" | "DELETE";
  body?: unknown;
  headers?: Record<string, string>;
  requiresAuth?: boolean;
  query?: Record<string, QueryValue>;
  retryOnUnauthorized?: boolean;
}

interface AuthBindings {
  getAccessToken: () => string | null;
  refreshSession: () => Promise<boolean>;
  clearSession: () => void;
}

const authBindings: AuthBindings = {
  getAccessToken: () => null,
  refreshSession: async () => false,
  clearSession: () => undefined,
};

export function configureHttpClient(bindings: Partial<AuthBindings>) {
  Object.assign(authBindings, bindings);
}

function buildUrl(path: string, query?: Record<string, QueryValue>) {
  const baseUrl = env.apiBaseUrl.endsWith("/") ? env.apiBaseUrl.slice(0, -1) : env.apiBaseUrl;
  const normalizedPath = path.startsWith("/") ? path : `/${path}`;
  const url = new URL(`${baseUrl}${normalizedPath}`);

  if (query) {
    for (const [key, value] of Object.entries(query)) {
      if (Array.isArray(value)) {
        value.filter((item) => item !== undefined && item !== null).forEach((item) => url.searchParams.append(key, String(item)));
        continue;
      }

      if (value !== undefined && value !== null) {
        url.searchParams.set(key, String(value));
      }
    }
  }

  return url.toString();
}

async function request<TResponse>(path: string, options: RequestOptions = {}): Promise<TResponse> {
  const {
    method = "GET",
    body,
    headers = {},
    requiresAuth = false,
    query,
    retryOnUnauthorized = true,
  } = options;

  const requestHeaders = new Headers(headers);
  requestHeaders.set("Accept", "application/json");

  const accessToken = requiresAuth ? authBindings.getAccessToken() : null;
  if (accessToken) {
    requestHeaders.set("Authorization", `Bearer ${accessToken}`);
  }

  const hasBody = body !== undefined;
  if (hasBody) {
    requestHeaders.set("Content-Type", "application/json");
  }

  const response = await fetch(buildUrl(path, query), {
    method,
    headers: requestHeaders,
    body: hasBody ? JSON.stringify(body) : undefined,
  });

  if (response.status === 401 && requiresAuth && retryOnUnauthorized) {
    const refreshed = await authBindings.refreshSession();

    if (refreshed) {
      return request<TResponse>(path, { ...options, retryOnUnauthorized: false });
    }

    authBindings.clearSession();
  }

  if (!response.ok) {
    throw await parseApiError(response);
  }

  if (response.status === 204) {
    return undefined as TResponse;
  }

  const contentType = response.headers.get("content-type");
  if (!contentType?.includes("application/json")) {
    throw new ApiClientError(response.status, "Resposta inesperada do servidor.");
  }

  return (await response.json()) as TResponse;
}

export const httpClient = {
  get: <TResponse>(path: string, options?: Omit<RequestOptions, "method" | "body">) =>
    request<TResponse>(path, { ...options, method: "GET" }),
  post: <TResponse>(path: string, body?: unknown, options?: Omit<RequestOptions, "method" | "body">) =>
    request<TResponse>(path, { ...options, method: "POST", body }),
  put: <TResponse>(path: string, body?: unknown, options?: Omit<RequestOptions, "method" | "body">) =>
    request<TResponse>(path, { ...options, method: "PUT", body }),
  delete: <TResponse>(path: string, options?: Omit<RequestOptions, "method" | "body">) =>
    request<TResponse>(path, { ...options, method: "DELETE" }),
};
