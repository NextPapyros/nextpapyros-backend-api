# ETAPA 1: BUILD
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

# Copiar archivos de solución y proyectos para restaurar dependencias
COPY *.sln .
COPY src/NextPapyros.API/*.csproj ./src/NextPapyros.API/
COPY src/NextPapyros.Application/*.csproj ./src/NextPapyros.Application/
COPY src/NextPapyros.Domain/*.csproj ./src/NextPapyros.Domain/
COPY src/NextPapyros.Infrastructure/*.csproj ./src/NextPapyros.Infrastructure/

# Restaurar dependencias (esto se cacheará si no cambian los .csproj)
RUN dotnet restore

# Copiar el resto del código
COPY . .

# Publicar la aplicación
RUN dotnet publish "./src/NextPapyros.API/NextPapyros.API.csproj" -c Release -o /app/out

# ETAPA 2: RUNTIME (Imagen ligera para producción)
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

# Configuración para Coolify/Producción
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080

ENTRYPOINT ["dotnet", "NextPapyros.API.dll"]