using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace CashControl.Core.CrossCutting;

/// <summary>
/// Classe contendo todas as configurações dinâmicas usadas pelo Core
/// </summary>
public class CoreSettings
{
    /// <summary>
    /// Interface IConfiguration, utilizada em diversas configurações do bootstrapper
    /// </summary>
    public IConfiguration? Configuration { get; set; }

    #region CrossCutting

    /// <summary>
    /// Configura a injeção da interface IHttpContextAccessor
    /// </summary>
    public bool ConfigureIHttpContextAccessor { get; set; } = true;

    /// <summary>
    /// Interface IHostEnvironment, utilizada para a correta configuração da interface IEnvironment
    /// </summary>
    public IHostEnvironment? HostEnvironment { get; set; }

    /// <summary>
    /// Configura a injeção da interface IEnvironment
    /// Depende da configuração correta da propriedade IHostEnvironment e IConfiguration
    /// </summary>
    public bool ConfigureIEnvironment { get; set; } = true;

    /// <summary>
    /// Configura a injeção da interface ILoggedUserProvider
    /// </summary>
    public bool ConfigureILoggedUserProvider { get; set; } = true;

    #endregion CrossCutting

    #region Api

    /// <summary>
    /// Configura a injeção do cache em memória
    /// </summary>
    public bool ConfigureMemoryCache { get; set; } = true;

    /// <summary>
    /// Configura o nome padrão do endpoint de health check
    /// </summary>
    public string HealthCheckEndpoint { get; set; } = "/health";

    /// <summary>
    /// Configura a injeção dos health checks padrões
    /// </summary>
    public bool ConfigureHealthChecks { get; set; } = true;

    /// <summary>
    /// Configura a injeção da compressão via gzip
    /// </summary>
    public bool ConfigureGzipCompression { get; set; } = true;

    /// <summary>
    /// Configura o uso da página de exceção de desenvolvedor quando em ambiente de desenvolvimento
    /// </summary>
    public bool ConfigureUseDeveloperExceptionPageWhenInDevelopmentEnvironment { get; set; }

    /// <summary>
    /// Configura a injeção do middleware de exceção
    /// </summary>
    public bool ConfigureExceptionMiddleware { get; set; } = true;

    /// <summary>
    /// Configura o uso do middleware de rate limiter.
    /// </summary>
    public bool ConfigureRateLimiter { get; set; }

    /// <summary>
    /// Registra a autenticação e autorização para o serviço.
    /// </summary>
    public bool RegisterAuthenticationAndAuthorization { get; set; } = true;

    /// <summary>
    /// Verifica se registra o Swagger UI para o serviço.
    /// </summary>
    public bool RegisterSwagger => SwaggerSettings != null;

    /// <summary>
    /// Configurações do Swagger UI..
    /// </summary>
    public SwaggerSettings? SwaggerSettings { get; set; }

    /// <summary>
    /// Adiciona os controllers padrão da API.
    /// </summary>
    public bool AddDefaultControllers { get; set; } = true;

    /// <summary>
    /// Utiliza configuração padrão para retorno de Json pela API.
    /// Padrão: 
    /// ReferenceLoopHandling.Ignore
    /// CamelCasePropertyNamesContractResolver
    /// </summary>
    public bool AddDefaultNewtonsoftJson { get; set; } = true;

    /// <summary>
    /// Utilizar middleware padrão para routing.
    /// </summary>
    public bool UseDefaultRouting { get; set; } = true;

    /// <summary>
    /// Utilizar middleware padrão para Endpoints.
    /// </summary>
    public bool UseDefaultEndpoints { get; set; } = true;

    /// <summary>
    /// Utilizar sistema de telemetria
    /// </summary>
    public bool UseTelemetry { get; set; } = true;

    #endregion Api

    #region Application

    /// <summary>
    /// Configura a injeção do pacote Mediator
    /// </summary>
    public bool ConfigureMediator { get; set; } = true;

    /// <summary>
    /// Nome do assembly do projeto de aplicação
    /// ex: ManyMinds.Modelo.Application
    /// </summary>
    public string ApplicationAssemblyName { get; set; } = string.Empty;

    /// <summary>
    /// Configura a injeção da abstração do mediator do Core
    /// </summary>
    public bool ConfigureCoreMediator { get; set; } = true;

    /// <summary>
    /// Configura a injeção de todos os handlers do mediator presentes na aplicação, commands, queries e events
    /// Depende da configuração correta do nome do assembly da camada de aplicação - ApplicationAssemblyName
    /// </summary>
    public bool ConfigureMediatorHandlers { get; set; } = true;

    /// <summary>
    /// Configura a injeção de todos os pipeline behaviors do mediator presentes no Core
    /// Depende da configuração correta do nome do assembly da camada de aplicação - ApplicationAssemblyName
    /// </summary>
    public bool ConfigureMediatorPipelineBehaviors { get; set; } = true;

    /// <summary>
    /// Configura a injeção do broker de mensageria para os eventos
    /// </summary>
    public bool ConfigureBroker { get; set; } = true;

    /// <summary>
    /// Tipo da classe do startup do serviço.
    /// </summary>
    public Type? TypeStartUp { get; set; }

    /// <summary>
    /// Determina se a aplicação vai ter configurações de multi-tenant ou não
    ///
    /// Observe que sendo "true", não significa que a aplicação está pronta
    /// para ser multi-tenant, mas, sim, que algumas configurações serão alteradas
    /// </summary>
    public bool IsMultiTenancyEnabled { get; set; } = true;

    /// <summary>
    /// Usa mensageria do RabbitMq
    /// </summary>
    public bool MessagesRabbitMq { get; set; } = true;

    #endregion Application

    #region Infra

    /// <summary>
    /// Nome da string de conexão a base de dados
    /// </summary>
    public string ConnectionStringName { get; set; } = "DefaultConnection";

    /// <summary>
    /// Configura a injeção da interface IGetRepository
    /// </summary>
    public bool ConfigureIGetRepository { get; set; } = true;

    /// <summary>
    /// Configura a injeção da interface IGenericRepository
    /// </summary>
    public bool ConfigureIGenericRepository { get; set; } = true;

    #endregion Infra
}
