# CashControl

Microservico de identidade do projeto CashControl, implementado em `.NET 8` com `ASP.NET Core`, `MediatR`, `ASP.NET Identity`, `Entity Framework Core` e `JWT`.

O projeto foi organizado com foco em `DDD`, `Clean Code` e `Onion Architecture`, separando responsabilidades entre `API`, `Application`, `Domain`, `Infra` e um `Core` compartilhado.

## Visao Geral

A solucao atualmente concentra o contexto de identidade:

- autenticacao com `JWT`
- cadastro e login de usuarios
- refresh token
- confirmacao de email
- recuperacao e reset de senha
- gestao do perfil do usuario autenticado
- gestao administrativa de usuarios e papeis

## Estrutura

```text
src/
  CashControl.Core/
  Identity/
    CashControl.Identity.API/
    CashControl.Identity.Application/
    CashControl.Identity.Domain/
    CashControl.Identity.Infra/
tests/
  CashControl.Identity.API.IntegrationTests/
```

### Camadas

- `CashControl.Identity.API`
  Exposicao HTTP, contratos de request e controllers.

- `CashControl.Identity.Application`
  Commands, queries, handlers, validators e contratos de servico.

- `CashControl.Identity.Domain`
  Entidades e regras centrais do dominio.

- `CashControl.Identity.Infra`
  Implementacao concreta do `IIdentityService`, `DbContext`, integracao com Identity e persistencia.

- `CashControl.Core`
  Infraestrutura compartilhada: middleware, bootstrap, autenticacao, swagger, pipeline do MediatR, health checks e helpers de resposta.

## Arquitetura

O fluxo principal da requisicao segue este caminho:

```text
HTTP Request
-> Controller
-> MediatR
-> Command/Query Handler
-> IIdentityService
-> ASP.NET Identity / EF Core
-> MediatorResult
-> HTTP Response
```

Direcao das dependencias:

- `API` depende de `Application` e `Infra`
- `Infra` depende de `Application` e `Domain`
- `Application` depende apenas de `Core`
- `Domain` depende apenas de `Core`

Isso evita acoplamento da camada de aplicacao com detalhes de infraestrutura.

## Principais Tecnologias

- `.NET 8`
- `ASP.NET Core`
- `MediatR`
- `FluentValidation`
- `ASP.NET Core Identity`
- `Entity Framework Core`
- `SQL Server`
- `SQLite` para testes de integracao
- `Swagger`

## Endpoints

### Auth

Base route: `v1/auth`

- `POST /v1/auth/register`
- `POST /v1/auth/login`
- `POST /v1/auth/refresh-token`
- `POST /v1/auth/forgot-password`
- `POST /v1/auth/reset-password`
- `POST /v1/auth/confirm-email`

### Usuario autenticado

Base route: `v1/users`

- `GET /v1/users/me`
- `PUT /v1/users/me`
- `POST /v1/users/me/change-password`
- `DELETE /v1/users/me/refresh-token`

### Administracao

Base route: `v1/admin/users`

- `GET /v1/admin/users/{userId}`
- `GET /v1/admin/users/{userId}/roles`
- `PUT /v1/admin/users/{userId}/roles/{role}`
- `DELETE /v1/admin/users/{userId}/roles/{role}`
- `DELETE /v1/admin/users/{userId}`

Os endpoints administrativos exigem um usuario autenticado com role `Admin`.

## Configuracao

O arquivo principal de configuracao da API esta em [appsettings.json](/C:/Projetos/CashControl/src/Identity/CashControl.Identity.API/appsettings.json:1).

Exemplo atual:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=MinhaApiDb;User Id=sa;Password=SuaSenha@123;TrustServerCertificate=True"
  },
  "Jwt": {
    "Key": "SUA_CHAVE_SUPER_SECRETA_COM_PELO_MENOS_32_CARACTERES",
    "Issuer": "MinhaApi",
    "Audience": "MinhaApiClient",
    "AccessTokenMinutes": 120,
    "RefreshTokenDays": 7
  }
}
```

Antes de rodar localmente:

1. Ajuste a `DefaultConnection` para o seu SQL Server.
2. Defina uma chave JWT real com pelo menos 32 caracteres.
3. Revise `Issuer` e `Audience` conforme o ambiente.

## Como Executar

Na raiz da solucao:

```bash
dotnet restore
dotnet build CashControl.sln
dotnet run --project src/Identity/CashControl.Identity.API/CashControl.Identity.API.csproj
```

Com a API em execucao, o Swagger fica disponivel no endpoint padrao configurado pelo Core.

## Testes

O projeto usa testes de integracao com `WebApplicationFactory` e `SQLite` em memoria.

Projeto de testes:

- [CashControl.Identity.API.IntegrationTests.csproj](/C:/Projetos/CashControl/tests/CashControl.Identity.API.IntegrationTests/CashControl.Identity.API.IntegrationTests.csproj:1)

Executar todos os testes:

```bash
dotnet test CashControl.sln
```

A cobertura atual valida todos os endpoints publicos do microservico de identidade.

## Pontos de Entrada Importantes

- Inicializacao da API: [Program.cs](/C:/Projetos/CashControl/src/Identity/CashControl.Identity.API/Program.cs:1)
- Bootstrap compartilhado: [Bootstrapper.cs](/C:/Projetos/CashControl/src/CashControl.Core/CrossCutting/Bootstrapper.cs:1)
- Implementacao da regra de identidade: [IdentityService.cs](/C:/Projetos/CashControl/src/Identity/CashControl.Identity.Infra/Services/IdentityService.cs:1)

## Observacoes

- O projeto usa `ASP.NET Identity` como base para usuarios, senha, roles e tokens de confirmacao/reset.
- O `refresh token` e sua expiracao sao persistidos na entidade `User`.
- A serializacao de respostas passa pelo `BaseControllerHelper` do Core.
- A telemetria com Elastic APM foi registrada via DI para evitar uso de API obsoleta.

## Estado Atual

- build da solucao sem warnings
- testes de integracao cobrindo todos os endpoints
- arquitetura ajustada para reduzir acoplamento entre camadas
