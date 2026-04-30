# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY src/ ./src/
RUN dotnet publish ./src/Pdv.API/Pdv.API.csproj -c Release -o /app/publish

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 5190
ENV ASPNETCORE_URLS=http://+:5190
ENTRYPOINT ["dotnet", "Pdv.API.dll"]
