# PDV + Estoque (MVP)

Sistema de PDV com estoque — [.NET 8](backend/) + [React + Vite](frontend/pdv-web/). Documentação de domínio: [`agents.md`](agents.md), roadmap: [`roadmap.md`](roadmap.md), erros HTTP: [`docs/api-errors.md`](docs/api-errors.md).

## Status atual de multitenancy

O projeto está em transição para **multitenancy com isolamento lógico** (shared database, shared schema).

- Implementado no backend: `TenantId` nas entidades principais de negócio/autenticação.
- JWT agora inclui claims `tenant_id` e `is_super_admin`.
- Contexto de tenant por request e filtro global EF Core para entidades tenant-scoped.
- Migration criada: `StartLogicalMultitenancy` (backfill padrão para tenant `1`).
- Frontend já persiste `tenantId` na sessão.

Observação: nesta fase do MVP, a UI segue com **1 tenant por sessão/login** (sem seletor de tenant).

## Pré-requisitos

- [.NET SDK 8](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) (recomendado 20.19+ ou 22 LTS)
- [Docker](https://docs.docker.com/get-docker/) + Docker Compose (opcional — stack completo com SQL Server e frontend estático)
- SQL Server só é necessário se `Database:UseInMemory` estiver `false`. Em desenvolvimento o padrão é **`Database:UseInMemory: true`** (EF Core InMemory), sem precisar de instância SQL para subir a API localmente.

## Docker (API + SQL Server + frontend)

Na raiz do repositório:

1. Copie as variáveis e ajuste segredos:

   ```bash
   cp .env.example .env
   ```

   O arquivo **[`.env`](.env)** (gitignored) concentra credenciais e segredos:

   | Variável | Uso |
   |----------|-----|
   | `MSSQL_SA_PASSWORD` | Senha do usuário `sa` no SQL Server (política forte: maiúscula, minúscula, número e símbolo). Deve ser a mesma usada em `ConnectionStrings__DefaultConnection` (o Compose preenche a connection string com este valor). |
   | `JWT_KEY` | Chave simétrica do JWT (use valor longo; não compartilhe em produção). |
   | `SEED_SUPERADMIN_PASSWORD` | Senha inicial do usuário seed (`SEED_SUPERADMIN_EMAIL`). |
   | `VITE_API_URL` | URL da API **como o navegador a alcança** (ex.: `http://localhost:5190`). É embutida no build do frontend. |
   | `CORS_ORIGIN` | Origem do frontend permitida na API (ex.: `http://localhost:8080`). Se mudar `WEB_PORT`, atualize também `CORS_ORIGIN`. |

   Portas padrão: API `5190`, web `8080`, SQL `1433` (`API_PORT`, `WEB_PORT`, `SQL_PORT`).

2. Subir tudo:

   ```bash
   docker compose up --build
   ```

3. URLs úteis:

   - Frontend: `http://localhost:8080` (ajuste se `WEB_PORT` for outro)
   - API / health: `http://localhost:5190/api/health`
   - Na primeira subida a API aplica **migrations** e roda o **seed** (ver `backend/src/Pdv.API/Program.cs`).

Arquivos relacionados: [`docker-compose.yml`](docker-compose.yml), [`backend/Dockerfile`](backend/Dockerfile), [`frontend/pdv-web/Dockerfile`](frontend/pdv-web/Dockerfile).

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

### Testes

```bash
cd backend
dotnet test tests/Pdv.Tests/Pdv.Tests.csproj
```

Inclui testes de comandos/handlers e integração HTTP da API (`WebApplicationFactory`). Ver [`docs/design/stitch-phase7-tests-ui.md`](docs/design/stitch-phase7-tests-ui.md).

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

### Testes (Vitest)

```bash
cd frontend/pdv-web
npm test
```

### Autenticação (Fase 1)

- Endpoints: `POST /api/auth/login`, `POST /api/auth/refresh` (JWT + refresh token no banco).
- Resposta de sessão inclui `tenantId`.
- Seed inicial (`Seed` em `appsettings.json`): permissões base, role Super Admin e usuário definido por `SuperAdminEmail` / `SuperAdminPassword`.
- Na subida, a API aplica migrations e executa o seed (veja `backend/src/Pdv.API/Program.cs`).

### Multitenancy (Fase 9 - em andamento)

- Isolamento lógico por `TenantId` em leituras/escritas do backend.
- Claims de tenant no JWT: `tenant_id`, `is_super_admin`.
- Base para segregação de dados entre tenants já ativa em infraestrutura.

## Fase 0 (entrega)

- Solução em camadas com MediatR, FluentValidation, EF Core + migration inicial
- Cliente HTTP (`axios`) + store (`zustand`) e hook de health check no frontend

## Fase 1 (entrega)

- Entidades User / Role / Permission, login JWT, refresh token, seed Super Admin
- Frontend: tela de login (referência visual: [`frontend/pdv-web/docs/STITCH_LOGIN.md`](frontend/pdv-web/docs/STITCH_LOGIN.md)), rotas protegidas, `can()` / `usePermission()`, sessão com `zustand` persist
