using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace CashControl.Core.HealthCheck;

public class ClientHttpStatusInfoHealthCheck(string urlAddress) : IHealthCheck
{
    private readonly string _urlAddress = urlAddress ?? throw new ArgumentNullException(nameof(urlAddress));

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default(CancellationToken))
    {
        var _httpClient = new HttpClient();
        var response = _httpClient.GetAsync(_urlAddress, cancellationToken);

        var result = response.Result.StatusCode != System.Net.HttpStatusCode.OK
            ? context.Registration.FailureStatus
            : HealthStatus.Healthy;

        var data = new Dictionary<string, object>()
        {
            {"StatusCode", response.Result.StatusCode }
        };

        if (response.Result.StatusCode != System.Net.HttpStatusCode.OK)
        {
            data.Add("Content", response.Result.Content.ReadAsStringAsync(cancellationToken).Result);
            data.Add("Url", _urlAddress);
        }

        return Task.FromResult(new HealthCheckResult(
            result,
            description: "Valida status http para " + context.Registration.Name,
            data: data));
    }
}

public class ClientHttpStatusInfoOptions
{
    public string UrlAddress { get; set; } = string.Empty;
}
