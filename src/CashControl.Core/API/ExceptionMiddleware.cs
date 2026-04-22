using CashControl.Core.CrossCutting;
using Elastic.Apm;
using Elastic.Apm.Api;
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

        ITransaction? transaction = null;

        if (useTelemetry)
            transaction = Agent.Tracer.StartTransaction(
                context.Request?.Path.Value ??
                context.GetEndpoint()?.DisplayName ??
                "Core Default Transaction", ApiConstants.TypeRequest);

        try
        {
            context.Response.Headers.Append("X-TELEMETRY", useTelemetry ? "ENABLED" : "DISABLED");

            if (useTelemetry)
                context.Response.Headers.Append("X-TELEMETRY-TRANSACTION-ID", Agent.Tracer.CurrentTransaction.Id);

            await _next(context);
        }
        catch (CustomException ex)
        {
            if (useTelemetry && transaction != null)
                transaction.CaptureException(ex, JsonConvert.SerializeObject(ex.Errors));

            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            await context.Response.WriteAsJsonAsync(ex.Errors);
        }
        catch (Exception e)
        {
            if (useTelemetry && transaction != null)
                transaction.CaptureException(e);

            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        }
        finally
        {
            if (useTelemetry && transaction != null)
                transaction.End();
        }
    }
}