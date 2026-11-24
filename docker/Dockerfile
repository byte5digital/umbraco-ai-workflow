# syntax=docker/dockerfile:1
# Build context: repository root (see docker/docker-compose.yml)

# 1) Tailwind CSS
FROM node:22-alpine AS css
WORKDIR /src
COPY package.json package-lock.json ./
RUN npm ci --ignore-scripts
COPY src/Opten.Web/Styles ./src/Opten.Web/Styles
COPY src/Opten.Web/Views ./src/Opten.Web/Views
COPY src/Opten.Web/Site ./src/Opten.Web/Site
RUN npm run build:css

# 2) .NET build & publish
FROM mcr.microsoft.com/dotnet/sdk:11.0 AS build
WORKDIR /src
COPY global.json Directory.Build.props Directory.Packages.props .editorconfig ./
COPY src/Opten.Web/Opten.Web.csproj ./src/Opten.Web/
RUN dotnet restore src/Opten.Web/Opten.Web.csproj
COPY src/Opten.Web/ ./src/Opten.Web/
COPY --from=css /src/src/Opten.Web/wwwroot/css/site.css ./src/Opten.Web/wwwroot/css/site.css
RUN dotnet publish src/Opten.Web/Opten.Web.csproj -c Release --no-restore -p:SkipTailwind=true -o /app/publish

# 3) Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Umbraco writes media, logs and data (SQLite) below these folders – mounted as volumes
RUN mkdir -p /app/wwwroot/media /app/umbraco/Logs /app/umbraco/Data /home/app/.aspnet/DataProtection-Keys \
    && chown -R app:app /app /home/app/.aspnet

USER app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
HEALTHCHECK --interval=30s --timeout=5s --start-period=60s --retries=5 \
    CMD wget -qO- http://localhost:8080/robots.txt > /dev/null || exit 1

ENTRYPOINT ["dotnet", "Opten.Web.dll"]
