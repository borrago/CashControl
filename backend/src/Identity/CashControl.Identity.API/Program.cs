using CashControl.Core.API;
using CashControl.Core.CrossCutting;
using CashControl.Identity.Infra;
using CashControl.Identity.Infra.Options;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

var cancellationToken = new CancellationToken();
var builder = WebApplication.CreateBuilder(args);

var rootDomain = "Identity";
var coreSettings = new CoreSettings
{
    HostEnvironment = builder.Environment,
    Configuration = builder.Configuration,
    ApplicationAssemblyName = "CashControl.Identity.Application",
    SwaggerSettings = new SwaggerSettings(rootDomain, $"Micro-Servico relacionado aos aspectos do dominio de {rootDomain}."),
    TypeStartUp = typeof(Program),
    ConfigureUseDeveloperExceptionPageWhenInDevelopmentEnvironment = true,
    ConfigureRateLimiter = true,
    IsMultiTenancyEnabled = false,
};

builder.Services.AddInfrastructure(coreSettings);
var corsOptions = builder.Configuration.GetSection("Cors").Get<CorsOptionsSettings>() ?? new CorsOptionsSettings();
var topologyOptions = builder.Configuration.GetSection("DeploymentTopology").Get<DeploymentTopologyOptions>() ?? new DeploymentTopologyOptions();
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        if (corsOptions.AllowedOrigins.Length == 0)
            return;

        policy.WithOrigins(corsOptions.AllowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();

        if (corsOptions.AllowCredentials)
            policy.AllowCredentials();
    });
});
builder.Services.AddHsts(options =>
{
    options.Preload = true;
    options.IncludeSubDomains = true;
    options.MaxAge = TimeSpan.FromDays(180);
});

var securityOptions = builder.Configuration.GetSection("Security").Get<SecurityOptions>() ?? new SecurityOptions();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, rejectionToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsJsonAsync(
            ApiErrorResponse.TooManyRequests("Muitas requisicoes em pouco tempo. Aguarde antes de tentar novamente."),
            rejectionToken);
    };

    options.AddPolicy("AuthSensitive", httpContext =>
    {
        var partitionKey = httpContext.User.Identity?.Name
            ?? httpContext.Connection.RemoteIpAddress?.ToString()
            ?? "anonymous";

        return RateLimitPartition.GetFixedWindowLimiter(
            $"{httpContext.Request.Path}:{partitionKey}",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = securityOptions.AuthRateLimitPermitLimit,
                Window = TimeSpan.FromSeconds(securityOptions.AuthRateLimitWindowSeconds),
                QueueLimit = 0,
                AutoReplenishment = true
            });
    });
});

var app = builder.Build();

await app.Services.SeedIdentityDataAsync();

if (!app.Environment.IsDevelopment() && !app.Environment.IsEnvironment("Test") && topologyOptions.RequireHttps)
    app.UseHsts();

app.UseHttpsRedirection();
if (corsOptions.AllowedOrigins.Length > 0)
    app.UseCors("Frontend");
app.UseCore(coreSettings);

await app.RunAsync(cancellationToken);

public partial class Program;
