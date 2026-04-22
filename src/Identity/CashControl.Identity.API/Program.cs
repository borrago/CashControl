using CashControl.Core.CrossCutting;
using CashControl.Identity.Infra;

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
};

builder.Services.AddInfrastructure(coreSettings);

var app = builder.Build();

app.UseHttpsRedirection();
app.UseCore(coreSettings);

await app.RunAsync(cancellationToken);

public partial class Program;
