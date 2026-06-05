# Backend PDV

Solução [`Pdv.sln`](Pdv.sln): **Domain**, **Application** (CQRS + MediatR + FluentValidation), **Infrastructure** (EF Core + SQL Server), **API**.

## Multitenancy (status atual)

Implementação inicial de isolamento lógico por tenant já aplicada:

- Entidades principais possuem `TenantId`.
- `AppDbContext` aplica filtro global para entidades tenant-scoped.
- JWT inclui `tenant_id` e `is_super_admin`.
- Seed e migration inicial de multitenancy disponíveis.

Migration de referência: `StartLogicalMultitenancy`.

## Comandos úteis

| Ação | Comando |
|------|---------|
| Build | `dotnet build` |
| Testes | `dotnet test tests/Pdv.Tests/Pdv.Tests.csproj` |
| Rodar API | `dotnet run --project src/Pdv.API/Pdv.API.csproj --launch-profile http` |
| Nova migration | `dotnet ef migrations add Nome --project src/Pdv.Infrastructure/Pdv.Infrastructure.csproj --startup-project src/Pdv.API/Pdv.API.csproj --output-dir Persistence/Migrations` |
| Aplicar DB | `dotnet ef database update --project src/Pdv.Infrastructure/Pdv.Infrastructure.csproj --startup-project src/Pdv.API/Pdv.API.csproj` |

Requer `dotnet-ef`: `dotnet tool install --global dotnet-ef` (versão alinhada ao EF Core 8).

## Pacotes principais

- Application: MediatR, FluentValidation  
- Infrastructure: `Microsoft.EntityFrameworkCore.SqlServer` 8.x, Design  
- API: `Microsoft.EntityFrameworkCore.Design` (design-time / EF tools)

O pacote `MediatR.Extensions.Microsoft.DependencyInjection` está **deprecado**; o registro usa `AddMediatR` incluído no pacote **MediatR** atual.

## Observações de auth/sessão

- Endpoints: `POST /api/auth/login` e `POST /api/auth/refresh`.
- Resposta inclui `tenantId` além de token/permissões.
