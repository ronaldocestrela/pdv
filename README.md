# PDV + Estoque (MVP)

Sistema de PDV com estoque — [.NET 8](backend/) + [React + Vite](frontend/pdv-web/). Documentação de domínio: [`agents.md`](agents.md), roadmap: [`roadmap.md`](roadmap.md).

## Pré-requisitos

- [.NET SDK 8](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) (recomendado 20.19+ ou 22 LTS)
- SQL Server só é necessário se `Database:UseInMemory` estiver `false`. Em desenvolvimento o padrão é **`Database:UseInMemory: true`** (EF Core InMemory), sem precisar de instância SQL para subir a API localmente.

## Backend

```bash
cd backend
dotnet restore
dotnet build
```

Executar API (HTTP em `http://localhost:5190`):

```bash
dotnet run --project src/Pdv.API/Pdv.API.csproj --launch-profile http
```

Health check: `GET http://localhost:5190/api/health`

### Banco de dados

- **`Database:UseInMemory`** (`true`/`false`): em **`appsettings.Development.json`** está `true` por padrão — a API usa o provider **EF Core InMemory** (`EnsureCreated`), sem SQL Server nem `dotnet ef database update`.
- Com **`UseInMemory: false`**, configure `ConnectionStrings:DefaultConnection` e aplique migrations:

1. Ajuste `ConnectionStrings:DefaultConnection` em `backend/src/Pdv.API/appsettings.json` (ou use User Secrets / variáveis de ambiente).

2. Subir SQL Server (exemplo Docker):

   ```bash
   docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=Your_password123" \
     -p 1433:1433 --name pdv-sql -d mcr.microsoft.com/mssql/server:2022-latest
   ```

3. Aplicar migrations:

   ```bash
   cd backend
   dotnet ef database update --project src/Pdv.Infrastructure/Pdv.Infrastructure.csproj \
     --startup-project src/Pdv.API/Pdv.API.csproj
   ```

## Frontend

```bash
cd frontend/pdv-web
cp .env.example .env   # opcional — ajuste VITE_API_URL
npm install
npm run dev
```

Por padrão o Vite usa `http://localhost:1234`. O backend aceita CORS para essa origem (ver `appsettings.json`).

### Autenticação (Fase 1)

- Endpoints: `POST /api/auth/login`, `POST /api/auth/refresh` (JWT + refresh token no banco).
- Seed inicial (`Seed` em `appsettings.json`): permissões base, role Super Admin e usuário definido por `SuperAdminEmail` / `SuperAdminPassword`.
- Na subida, a API aplica migrations e executa o seed (veja `backend/src/Pdv.API/Program.cs`).

## Fase 0 (entrega)

- Solução em camadas com MediatR, FluentValidation, EF Core + migration inicial
- Cliente HTTP (`axios`) + store (`zustand`) e hook de health check no frontend

## Fase 1 (entrega)

- Entidades User / Role / Permission, login JWT, refresh token, seed Super Admin
- Frontend: tela de login (referência visual: [`frontend/pdv-web/docs/STITCH_LOGIN.md`](frontend/pdv-web/docs/STITCH_LOGIN.md)), rotas protegidas, `can()` / `usePermission()`, sessão com `zustand` persist
