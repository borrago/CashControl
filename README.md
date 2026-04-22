# CashControl

Monorepo com duas frentes principais:

- `backend/`: API .NET e testes de integração.
- `frontend/`: aplicação Vue 3 + TypeScript para consumir todos os endpoints disponíveis.

## Estrutura

```text
CashControl/
  backend/
  frontend/
```

## Backend

```powershell
cd backend
dotnet build CashControl.sln
dotnet test CashControl.sln
```

## Frontend

```powershell
cd frontend
npm install
npm run dev
```
