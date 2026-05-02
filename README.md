# CashControl

Monorepo com duas frentes:

- `backend/`: microservico de identidade em `.NET 8`
- `frontend/`: SPA em `Vue 3 + TypeScript`

## Estado atual

O sistema hoje cobre o contexto de identidade:

- cadastro e login
- confirmacao de e-mail
- recuperacao e reset de senha
- sessao web por cookie `HttpOnly`
- CSRF explicito para operacoes autenticadas de escrita
- perfil do usuario autenticado
- administracao de usuarios e papeis
- impersonacao de usuario por ator global (`IsSuperUser`), com retorno seguro para a sessao original

## Backend

```powershell
cd backend
dotnet build CashControl.sln
dotnet test CashControl.sln
```

Documentacao principal:

- [backend/README.md](backend/README.md)
- [backend/README.flows.md](backend/README.flows.md)
- [backend/README.flows.v2.md](backend/README.flows.v2.md)

## Frontend

```powershell
cd frontend
npm install
npm run dev
npm test
npm run build
```

## Contrato de autenticacao

O frontend nao usa mais `Bearer token`.

Fluxo atual:

1. `POST /v1/auth/login` cria a sessao via cookie.
2. O navegador envia o cookie automaticamente nas proximas requests.
3. `GET /v1/auth/csrf-token` devolve o token antiforgery.
4. Toda operacao autenticada de escrita envia o header `X-CSRF-TOKEN`.

## Topologia recomendada

Para producao, a topologia esperada e:

- app web em `https://app.cashcontrol.com`
- API em `https://api.cashcontrol.com`
- CORS restrito ao frontend oficial
- cookies `__Host-*` sem `Domain`
- HTTPS obrigatorio e `HSTS` habilitado
