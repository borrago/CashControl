# CashControl Flows

Este documento mapeia os fluxos existentes do sistema no estado atual da solucao, com foco no microservico de identidade.

## Escopo Atual

Hoje o repositorio implementa principalmente o contexto de `Identity`, cobrindo:

- autenticacao
- tokens JWT
- refresh token
- perfil do usuario autenticado
- confirmacao de email
- recuperacao de senha
- administracao de usuarios e roles

## Mapa Geral

Fluxo tecnico padrao:

```text
Request HTTP
-> Controller
-> MediatR
-> Command/Query Handler
-> IIdentityService
-> ASP.NET Identity / EF Core
-> MediatorResult
-> BaseControllerHelper
-> Response HTTP
```

Fluxo de infraestrutura:

```text
Program
-> AddInfrastructure(CoreSettings)
-> RegisterCore()
-> Register API/Application/CrossCutting
-> Pipeline ASP.NET Core
```

## Inicializacao Da Aplicacao

Ponto de entrada:

- [Program.cs](/C:/Projetos/CashControl/src/Identity/CashControl.Identity.API/Program.cs:1)

Fluxo:

1. Cria `CoreSettings` com ambiente, configuracao, assembly da camada `Application` e metadados de Swagger.
2. Registra `Infra` via `AddInfrastructure(coreSettings)`.
3. A infraestrutura registra `DbContext`, `ASP.NET Identity`, `IIdentityService` e o core compartilhado.
4. O app sobe com `UseHttpsRedirection()` e `UseCore(coreSettings)`.

## Pipeline HTTP

Arquivo principal:

- [Bootstrapper.cs](/C:/Projetos/CashControl/src/CashControl.Core/CrossCutting/Bootstrapper.cs:1)

O pipeline hoje faz:

1. `ExceptionMiddleware`
2. `DeveloperExceptionPage` em desenvolvimento
3. `HealthChecks`
4. `Swagger`
5. `Routing`
6. `Authentication`
7. `Authorization`
8. `MapDefaultControllerRoute`

## Tratamento De Erros

Arquivo:

- [ExceptionMiddleware.cs](/C:/Projetos/CashControl/src/CashControl.Core/API/ExceptionMiddleware.cs:1)

Fluxo:

1. Toda request passa pelo middleware.
2. Se `UseTelemetry` estiver ativo, a request abre uma transacao APM.
3. `CustomException` retorna `400 Bad Request` com a lista de erros.
4. Excecao nao tratada retorna `500 Internal Server Error`.
5. A resposta recebe headers de telemetria.

## Materializacao Das Respostas

Arquivo:

- [BaseControllerHelper.cs](/C:/Projetos/CashControl/src/CashControl.Core/API/BaseControllerHelper.cs:1)

Fluxo:

1. O controller devolve um `IMediatorResult`.
2. O helper converte isso em `IActionResult`.
3. Se houver erros de validacao, retorna `400`.
4. Se o status configurado for `204`, retorna `NoContent`.
5. Propriedades do resultado sao serializadas dinamicamente.
6. Colecoes simples, como `IList<string>`, sao retornadas como lista simples.

## Fluxos De Negocio

## 1. Cadastro De Usuario

Endpoint:

- `POST /v1/auth/register`

Entrada:

- email
- senha
- nome completo opcional

Controller:

- [AuthController.cs](/C:/Projetos/CashControl/src/Identity/CashControl.Identity.API/Controllers/AuthController.cs:20)

Fluxo:

1. Controller cria `RegisterCommandInput`.
2. MediatR encaminha para o handler.
3. `IdentityService.RegisterAsync(...)` normaliza email.
4. Verifica se ja existe usuario com o email.
5. Cria `User` com `EmailConfirmed = false`.
6. Executa `UserManager.CreateAsync(...)`.
7. Se der certo, gera access token e refresh token.
8. Persiste refresh token e expiracao no usuario.
9. Retorna tokens e data de expiracao.

Persistencia:

- tabela de usuarios do Identity
- campos adicionais `RefreshToken` e `RefreshTokenExpiryTimeUtc`

Saida:

- `200 OK`
- `AccessToken`
- `RefreshToken`
- `RefreshTokenExpiresAtUtc`

## 2. Login

Endpoint:

- `POST /v1/auth/login`

Fluxo:

1. Controller cria `LoginCommandInput`.
2. Service busca usuario por email.
3. Valida senha com `SignInManager.CheckPasswordSignInAsync(...)`.
4. Gera novo access token.
5. Gera novo refresh token.
6. Persiste refresh token no usuario.
7. Retorna payload de autenticacao.

Saida:

- `200 OK`
- novos tokens

## 3. Refresh Token

Endpoint:

- `POST /v1/auth/refresh-token`

Entrada:

- access token expirado
- refresh token atual

Fluxo:

1. Controller cria `RefreshTokenCommandInput`.
2. Service le o principal do token expirado sem validar lifetime.
3. Extrai `NameIdentifier` ou `sub`.
4. Busca usuario pelo id do token.
5. Compara refresh token recebido com o persistido.
6. Valida data de expiracao do refresh token.
7. Em caso valido, gera novo access token e novo refresh token.
8. Atualiza o usuario.
9. Retorna novos tokens.

Falhas principais:

- token invalido
- usuario inexistente
- refresh token invalido ou expirado

## 4. Revogar Refresh Token

Endpoint:

- `DELETE /v1/users/me/refresh-token`

Pre-condicao:

- usuario autenticado

Fluxo:

1. Controller extrai `ClaimTypes.NameIdentifier`.
2. Cria `RevokeRefreshTokenCommandInput`.
3. Service busca o usuario atual.
4. Limpa `RefreshToken` e `RefreshTokenExpiryTimeUtc`.
5. Persiste a atualizacao.

Saida:

- `204 No Content`

Efeito:

- o refresh token atual deixa de ser aceito

## 5. Confirmacao De Email

Endpoint:

- `POST /v1/auth/confirm-email`

Entrada:

- `userId`
- `token`

Fluxo:

1. Controller cria `ConfirmEmailCommandInput`.
2. Service busca usuario por id.
3. Executa `UserManager.ConfirmEmailAsync(...)`.
4. Em caso de sucesso, o email fica confirmado no usuario.

Saida:

- `204 No Content`

## 6. Recuperacao De Senha

Endpoint:

- `POST /v1/auth/forgot-password`

Fluxo:

1. Controller cria `ForgotPasswordCommandInput`.
2. Service busca usuario por email.
3. Executa `GeneratePasswordResetTokenAsync(...)`.
4. Retorna o token de reset.

Saida:

- `200 OK`
- token de reset

Observacao:

- hoje o fluxo retorna o token diretamente pela API; em um ambiente produtivo, o padrao seria enviar esse token por email.

## 7. Reset De Senha

Endpoint:

- `POST /v1/auth/reset-password`

Fluxo:

1. Controller cria `ResetPasswordCommandInput`.
2. Service busca usuario por email.
3. Executa `UserManager.ResetPasswordAsync(...)`.
4. Em caso de sucesso, a senha passa a ser a nova senha informada.

Saida:

- `204 No Content`

## 8. Consultar Usuario Logado

Endpoint:

- `GET /v1/users/me`

Pre-condicao:

- JWT valido

Fluxo:

1. Controller le o `userId` da claim `NameIdentifier`.
2. Cria `GetCurrentUserQueryInput`.
3. Service busca usuario pelo id.
4. Mapeia para `UserDto`.
5. Retorna dados do usuario e roles.

Saida:

- `200 OK`
- `Id`
- `Email`
- `UserName`
- `FullName`
- `PhoneNumber`
- `Roles`

## 9. Atualizar Perfil Do Usuario Logado

Endpoint:

- `PUT /v1/users/me`

Entrada:

- `FullName`
- `PhoneNumber`

Fluxo:

1. Controller resolve o usuario autenticado.
2. Cria `UpdateProfileCommandInput`.
3. Service busca o usuario.
4. Entidade `User` executa `UpdateProfile(...)`.
5. `UserManager.UpdateAsync(...)` persiste os dados.

Saida:

- `204 No Content`

## 10. Alterar Senha Do Usuario Logado

Endpoint:

- `POST /v1/users/me/change-password`

Entrada:

- senha atual
- nova senha

Fluxo:

1. Controller extrai o `userId` autenticado.
2. Cria `ChangePasswordCommandInput`.
3. Service busca o usuario.
4. Chama `UserManager.ChangePasswordAsync(...)`.
5. Se a senha atual ou a nova senha forem invalidas, gera erro de dominio/aplicacao.

Saida:

- `204 No Content`

## 11. Buscar Usuario Por Id

Endpoint:

- `GET /v1/admin/users/{userId}`

Pre-condicao:

- usuario autenticado com role `Admin`

Fluxo:

1. Controller cria `GetUserByIdQueryInput`.
2. Service busca usuario por id.
3. Mapeia os dados para `UserDto`.
4. Retorna dados e roles.

Saida:

- `200 OK`

## 12. Consultar Roles De Um Usuario

Endpoint:

- `GET /v1/admin/users/{userId}/roles`

Fluxo:

1. Controller cria `GetUserRolesQueryInput`.
2. Service busca usuario por id.
3. Executa `UserManager.GetRolesAsync(...)`.
4. Retorna a lista de roles.

Saida:

- `200 OK`
- `UserId`
- `Roles`

## 13. Atribuir Role

Endpoint:

- `PUT /v1/admin/users/{userId}/roles/{role}`

Fluxo:

1. Controller cria `AssignRoleCommandInput`.
2. Service busca usuario por id.
3. Normaliza a role.
4. Se a role nao existir, cria via `RoleManager.CreateAsync(...)`.
5. Se o usuario ja tiver a role, encerra sem erro.
6. Caso contrario, adiciona a role com `AddToRoleAsync(...)`.

Saida:

- `204 No Content`

## 14. Remover Role

Endpoint:

- `DELETE /v1/admin/users/{userId}/roles/{role}`

Fluxo:

1. Controller cria `RemoveRoleCommandInput`.
2. Service busca usuario.
3. Remove a role com `RemoveFromRoleAsync(...)`.

Saida:

- `204 No Content`

## 15. Excluir Usuario

Endpoint:

- `DELETE /v1/admin/users/{userId}`

Fluxo:

1. Controller cria `DeleteUserCommandInput`.
2. Service busca usuario.
3. Executa `UserManager.DeleteAsync(...)`.
4. Usuario e seus vinculos do Identity deixam de existir.

Saida:

- `204 No Content`

## Fluxos Internos Relevantes

## Validacao

As entradas passam por validators do FluentValidation registrados no pipeline do MediatR.

Fluxo:

1. Request entra no MediatR.
2. `FailFastPipelineBehavior` executa validators da request.
3. Se houver falha, uma `ApplicationException` e lancada.
4. O middleware converte isso em `400`.

## Emissao De JWT

Fluxo de geracao:

1. Service busca roles do usuario.
2. Monta claims:
   - `sub`
   - `NameIdentifier`
   - `Name`
   - `Email`
   - `jti`
   - `Role`
3. Assina o token com chave simetrica.
4. Usa expiracao baseada em `AccessTokenMinutes`.

## Persistencia Do Refresh Token

Fluxo:

1. Um refresh token aleatorio de 64 bytes e gerado.
2. O token e convertido para Base64.
3. O usuario recebe:
   - `RefreshToken`
   - `RefreshTokenExpiryTimeUtc`
4. O `UserManager.UpdateAsync(...)` persiste a alteracao.

## Regras De Autorizacao

Fluxos:

- `v1/auth/*`
  Nao exigem autenticacao.

- `v1/users/*`
  Exigem usuario autenticado.

- `v1/admin/users/*`
  Exigem usuario autenticado com role `Admin`.

## Entidade Principal

Arquivo:

- [User.cs](/C:/Projetos/CashControl/src/Identity/CashControl.Identity.Domain/Entities/User.cs:1)

Estado relevante do usuario:

- `Id`
- `Email`
- `UserName`
- `FullName`
- `PhoneNumber`
- `RefreshToken`
- `RefreshTokenExpiryTimeUtc`

Comportamentos atuais da entidade:

- `UpdateProfile(...)`
- `SetRefreshToken(...)`
- `RevokeRefreshToken()`

## Banco E Persistencia

Contexto:

- [Context.cs](/C:/Projetos/CashControl/src/Identity/CashControl.Identity.Infra/Context.cs:1)

Fluxo:

1. O contexto herda de `IdentityDbContext<User>`.
2. `ASP.NET Identity` gerencia tabelas de usuario, claims, login, roles e relacionamento usuario-role.
3. A entidade `User` adiciona colunas especificas do dominio de identidade do projeto.

## Mapa Rapido De Responsabilidades

- `Program`
  sobe a API e carrega o bootstrap

- `Bootstrapper`
  registra servicos cross-cutting, auth, swagger, MediatR e pipeline

- `Controllers`
  recebem HTTP e montam commands/queries

- `Application`
  representa casos de uso

- `IdentityService`
  concentra regra de identidade

- `ASP.NET Identity`
  executa operacoes de usuario, senha, token e role

## Estado Atual Dos Fluxos

Os fluxos existentes atualmente cobrem o ciclo completo de autenticacao e administracao basica de usuarios:

- entrada HTTP padronizada
- validacao por pipeline
- execucao de regra de negocio centralizada
- persistencia via Identity/EF Core
- resposta HTTP consistente
- cobertura por testes de integracao para todos os endpoints publicos
