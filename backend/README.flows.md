# CashControl Flows

Resumo dos fluxos implementados hoje no microservico de identidade.

## Fluxo tecnico base

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

## Inicializacao

1. `Program.cs` monta `CoreSettings`.
2. `AddInfrastructure(coreSettings)` registra `DbContext`, Identity, servicos e Core.
3. `Program.cs` registra `Cors`, `HSTS`, rate limiter e seed inicial.
4. `UseCore(coreSettings)` aplica middleware, auth e endpoints.

## Sessao web

1. `POST /v1/auth/login` cria a sessao no cookie `__Host-cashcontrol-session`.
2. `GET /v1/auth/csrf-token` emite o token antiforgery.
3. Requests autenticadas de escrita exigem `X-CSRF-TOKEN`.
4. `POST /v1/auth/refresh-token` renova a sessao atual.
5. `DELETE /v1/users/me/refresh-token` encerra a sessao.

## Rotas protegidas

- `v1/auth/*`
  Publicas, exceto `refresh-token`, que exige sessao autenticada.

- `v1/users/*`
  Exigem sessao autenticada.

- `v1/admin/users/*`
  Exigem sessao autenticada com role `Admin` ou claim `IsSuperUser=true`.

## CSRF

Protecao atual:

- endpoint de emissao: `GET /v1/auth/csrf-token`
- validacao: atributo `[ValidateCsrfToken]`
- escopo: operacoes autenticadas de escrita
- falha: `400` com payload de validacao padrao da API

## Impersonacao

Fluxo:

1. Um usuario com `IsSuperUser=true` chama `POST /v1/admin/users/{userId}/impersonate`.
2. O backend cria uma nova sessao com claims de impersonacao.
3. `GET /v1/users/me` passa a refletir o usuario impersonado.
4. `POST /v1/users/me/stop-impersonation` restaura a sessao original.

## Configuracao de deploy

Topologia recomendada:

- frontend: `https://app.cashcontrol.com`
- API: `https://api.cashcontrol.com`
- `Cors:AllowedOrigins` restrito ao frontend oficial
- `SessionCookie` e `Antiforgery` com `SecurePolicy=Always`
- `HSTS` ativo fora de `Development` e `Test`
