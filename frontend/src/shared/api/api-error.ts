export interface ApiErrorDetail {
  propertyName: string;
  errorMessage: string;
}

export interface ApiErrorResponse {
  code: string;
  message: string;
  errors: ApiErrorDetail[];
}

export class ApiClientError extends Error {
  public readonly statusCode: number;
  public readonly response?: ApiErrorResponse;

  public constructor(statusCode: number, message: string, response?: ApiErrorResponse) {
    super(message);
    this.name = "ApiClientError";
    this.statusCode = statusCode;
    this.response = response;
  }
}

export async function parseApiError(response: Response): Promise<ApiClientError> {
  let errorResponse: ApiErrorResponse | undefined;

  try {
    errorResponse = (await response.json()) as ApiErrorResponse;
  } catch {
    errorResponse = undefined;
  }

  return new ApiClientError(
    response.status,
    errorResponse?.message ?? `Falha na requisicao (${response.status}).`,
    errorResponse,
  );
}
