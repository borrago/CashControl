namespace CashControl.Core.CrossCutting;

/// <summary>
/// Classe contendo configurações necessárias ao Swagger UI.
/// </summary>
public class SwaggerSettings
{
    /// <summary>
    /// Construtor Swagger UI Settings.
    /// </summary>
    /// <param name="name">Nome do serviço para exibição no Swagger UI.</param>
    public SwaggerSettings(string name)
    {
        Name = name;
    }

    /// <summary>
    /// Construtor Swagger UI Settings.
    /// </summary>
    /// <param name="name">Nome do serviço para exibição no Swagger UI.</param>
    /// <param name="description">Descrição do serviço para exibição no Swagger UI.</param>
    public SwaggerSettings(string name, string description)
    {
        Name = name;
        Description = description;
    }

    /// <summary>
    /// Nome do serviço para exibição no Swagger.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Descrição do serviço para exibição no Swagger.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Url de endpoint do Swagger
    /// </summary>
    public string EndpointUrl { get; set; } = "/swagger/v1/swagger.json";

    /// <summary>
    /// Nome do endpoint do Swagger
    /// </summary>
    public string EndpointName { get; set; } = "ManyMinds Swagger Endpoint";
}