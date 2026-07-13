# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj and restore
COPY ["Pdv.Web.csproj", "./"]
RUN dotnet restore "Pdv.Web.csproj"

# Copy remaining source code and publish
COPY . .
RUN dotnet publish "Pdv.Web.csproj" -c Release -o /app/publish

# Runtime stage: host the static files in Nginx
FROM nginx:alpine
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/publish/wwwroot /usr/share/nginx/html
EXPOSE 80
