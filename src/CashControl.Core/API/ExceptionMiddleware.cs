using CashControl.Core.CrossCutting;
using Elastic.Apm;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Net;

namespace CashControl.Core.API;

public class ExceptionMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    public async Task InvokeAsync(HttpContext context, CoreSettings coreSettings)
    {
        var useTelemetry = coreSettings.UseTelemetry;

        try
        {
            await _next(context);
        }
        catch (CustomException ex)
        {
            if (useTelemetry)
                Agent.Tracer.CurrentTransaction?.CaptureException(ex, JsonConvert.SerializeObject(ex.Errors));

            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            await context.Response.WriteAsJsonAsync(ApiErrorResponse.Validation(ex.Errors));
        }
        catch (Exception ex)
        {
            if (useTelemetry)
                Agent.Tracer.CurrentTransaction?.CaptureException(ex);

            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            await context.Response.WriteAsJsonAsync(ApiErrorResponse.InternalServerError("Ocorreu um erro interno."));
        }
    }
}
