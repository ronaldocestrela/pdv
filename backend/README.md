# Backend PDV

Solução [`Pdv.sln`](Pdv.sln): **Domain**, **Application** (CQRS + MediatR + FluentValidation), **Infrastructure** (EF Core + SQL Server), **API**.

## Comandos úteis

| Ação | Comando |
|------|---------|
| Build | `dotnet build` |
| Rodar API | `dotnet run --project src/Pdv.API/Pdv.API.csproj --launch-profile http` |
| Nova migration | `dotnet ef migrations add Nome --project src/Pdv.Infrastructure/Pdv.Infrastructure.csproj --startup-project src/Pdv.API/Pdv.API.csproj --output-dir Persistence/Migrations` |
| Aplicar DB | `dotnet ef database update --project src/Pdv.Infrastructure/Pdv.Infrastructure.csproj --startup-project src/Pdv.API/Pdv.API.csproj` |

Requer `dotnet-ef`: `dotnet tool install --global dotnet-ef` (versão alinhada ao EF Core 8).

## Pacotes principais

- Application: MediatR, FluentValidation  
- Infrastructure: `Microsoft.EntityFrameworkCore.SqlServer` 8.x, Design  
- API: `Microsoft.EntityFrameworkCore.Design` (design-time / EF tools)

O pacote `MediatR.Extensions.Microsoft.DependencyInjection` está **deprecado**; o registro usa `AddMediatR` incluído no pacote **MediatR** atual.
