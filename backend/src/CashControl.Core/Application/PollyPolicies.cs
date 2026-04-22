using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;

namespace CashControl.Core.Application;

public static class PollyPolicies
{
    private const int RETRY_COUNT = 5;
    private const int RETRY_COUNT_BEFORE_BREAK_CIRCUIT = 6;
    private const int DURATION_OF_BREAK_IN_SECONDS = 30;

    public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(IServiceProvider services)
    {
        var logger = services.GetRequiredService<ILogger<HttpClient>>();

        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(response => response.StatusCode == System.Net.HttpStatusCode.NotFound)
            .WaitAndRetryAsync(
            retryCount: RETRY_COUNT,
            sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            onRetry: (outcome, timespan, retryAttempt, context) =>
            {
                if (outcome.Exception is not null)
                {
                    logger?.LogError($"An exception was thrown on retry number {retryAttempt}. {outcome.Exception}");
                }
                else
                {
                    logger?.LogWarning(
                    $"A non sucess status code ({outcome?.Result?.StatusCode}) was received on retry number {retryAttempt}." +
                    $" Delaying for {timespan.TotalSeconds} seconds before retrying again");
                }
            });
    }

    public static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy(IServiceProvider services)
    {
        var logger = services.GetRequiredService<ILogger<HttpClient>>();

        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(
            handledEventsAllowedBeforeBreaking: RETRY_COUNT_BEFORE_BREAK_CIRCUIT,
            durationOfBreak: TimeSpan.FromSeconds(DURATION_OF_BREAK_IN_SECONDS),
            onBreak: (outcome, timespan, context) =>
            {
                logger?.LogError($"Circuit is break for {timespan.TotalSeconds} seconds.");
            },
            onReset: (context) =>
            {
                logger?.LogWarning("Circuit closed.");
            });
    }
}
