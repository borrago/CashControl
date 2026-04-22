using CashControl.Core.CrossCutting;

namespace CashControl.Core.API;

public sealed class ApiErrorResponse
{
    public string Code { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public IReadOnlyCollection<ApiErrorDetail> Errors { get; init; } = [];

    public static ApiErrorResponse Validation(IEnumerable<CustomValidationFailure> errors)
    {
        var details = errors
            .Select(error => new ApiErrorDetail
            {
                PropertyName = error.PropertyName,
                ErrorMessage = error.ErrorMessage
            })
            .ToArray();

        return new ApiErrorResponse
        {
            Code = "validation_error",
            Message = "A requisicao contem erros de validacao.",
            Errors = details
        };
    }

    public static ApiErrorResponse Unauthorized(string message)
        => Single("unauthorized", message);

    public static ApiErrorResponse Forbidden(string message)
        => Single("forbidden", message, "Authorization");

    public static ApiErrorResponse TooManyRequests(string message)
        => Single("rate_limit_exceeded", message);

    public static ApiErrorResponse InternalServerError(string message)
        => Single("internal_error", message);

    private static ApiErrorResponse Single(string code, string message, string propertyName = "")
        => new()
        {
            Code = code,
            Message = message,
            Errors =
            [
                new ApiErrorDetail
                {
                    PropertyName = propertyName,
                    ErrorMessage = message
                }
            ]
        };
}

public sealed class ApiErrorDetail
{
    public string PropertyName { get; init; } = string.Empty;
    public string ErrorMessage { get; init; } = string.Empty;
}
