# PDV + Estoque (MVP)

Sistema de PDV com estoque — [.NET 8](backend/) + [React + Vite](frontend/pdv-web/). Documentação de domínio: [`agents.md`](agents.md), roadmap: [`roadmap.md`](roadmap.md).

## Pré-requisitos

- [.NET SDK 8](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) (recomendado 20.19+ ou 22 LTS)
- SQL Server (local ou container) para aplicar migrations

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

Por padrão o Vite usa `http://localhost:5173`. O backend aceita CORS para essa origem (ver `appsettings.json`).

## Fase 0 (entrega)

- Solução em camadas com MediatR, FluentValidation, EF Core + migration inicial
- Cliente HTTP (`axios`) + store (`zustand`) e hook de health check no frontend
