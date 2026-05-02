# CashControl Identity

Microservico de identidade do CashControl, implementado em `.NET 8` com:

- `ASP.NET Core`
- `ASP.NET Core Identity`
- `Entity Framework Core`
- `MediatR`
- `FluentValidation`

## Arquitetura

Estrutura atual:

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

Direcao das dependencias:

- `API` depende de `Application` e `Infra`
- `Infra` depende de `Application` e `Domain`
- `Application` depende de `Core`
- `Domain` depende de `Core`

## Modelo de autenticacao

O servico nao usa mais `Bearer token` para o frontend web.

Fluxo atual:

1. `POST /v1/auth/login` valida credenciais.
2. O backend cria uma sessao por cookie `HttpOnly`.
3. O frontend chama `GET /v1/auth/csrf-token`.
4. Toda operacao autenticada de escrita envia `X-CSRF-TOKEN`.
5. `[Authorize]` valida a sessao pelo cookie do Identity.

## Endpoints

### Auth

Base route: `v1/auth`

- `GET /v1/auth/csrf-token`
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
- `POST /v1/users/me/stop-impersonation`

### Administracao

Base route: `v1/admin/users`

- `GET /v1/admin/users/{userId}`
- `GET /v1/admin/users/{userId}/roles`
- `POST /v1/admin/users/{userId}/impersonate`
- `PUT /v1/admin/users/{userId}/roles/{role}`
- `DELETE /v1/admin/users/{userId}/roles/{role}`
- `DELETE /v1/admin/users/{userId}`

## Seguranca

Controles implementados:

- cookie de sessao `HttpOnly`
- CORS restrito por configuracao
- `HSTS` fora de desenvolvimento e teste
- CSRF explicito nas operacoes autenticadas de escrita
- rate limit nas rotas sensiveis de autenticacao
- lockout por repetidas tentativas invalidas
- protecao de cookies `__Host-*`

## Impersonacao

Regras atuais:

- apenas usuarios com `IsSuperUser = true` podem iniciar impersonacao
- nao e permitido impersonar outro usuario com `IsSuperUser = true`
- a sessao impersonada pode ser revertida por `POST /v1/users/me/stop-impersonation`
- a UI recebe apenas capacidades derivadas, como permissao para impersonar e restaurar sessao

Observacao:

- a role `SuperAdmin` foi descontinuada como fonte de verdade
- o privilegio global agora e controlado por `IsSuperUser`
- a policy HTTP das rotas administrativas aceita `Admin` ou `IsSuperUser`
- `IsSuperUser` permanece interno ao backend e nao faz parte do contrato publico da API

## Configuracao

Arquivo principal:

- [appsettings.json](/C:/Projetos/CashControl/backend/src/Identity/CashControl.Identity.API/appsettings.json)

Secoes relevantes:

- `DeploymentTopology`
- `Cors`
- `SessionCookie`
- `Antiforgery`
- `Email`
- `Security`

Observacao importante:

- se o cookie usar prefixo `__Host-`, nao configure `Domain`

## Execucao

```powershell
cd backend
dotnet restore
dotnet build CashControl.sln
dotnet run --project src\\Identity\\CashControl.Identity.API\\CashControl.Identity.API.csproj
```

## Testes

```powershell
cd backend
dotnet test CashControl.sln
```

Cobertura atual:

- `23/23` testes de integracao passando
- fluxos autenticados e administrativos
- protecao CSRF
- impersonacao e retorno para a sessao original
