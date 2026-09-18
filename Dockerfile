FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /src

# Copiar proyectos primero para aprovechar la caché de Docker
COPY BackendAwSmartstay.API/BackendAwSmartstay.API.csproj BackendAwSmartstay.API/
COPY BackendAwSmartstay.Domain/BackendAwSmartstay.Domain.csproj BackendAwSmartstay.Domain/

# Restaurar dependencias
RUN dotnet restore BackendAwSmartstay.API/BackendAwSmartstay.API.csproj

# Copiar el código fuente (ver .dockerignore: sin bin/obj, .git, tests ni secretos)
COPY . .

# Compilar y publicar
RUN dotnet publish BackendAwSmartstay.API/BackendAwSmartstay.API.csproj \
    -c Release \
    -o /app/publish \
    --no-restore


FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

# Render utilizará este puerto
ENV ASPNETCORE_URLS=http://+:10000

EXPOSE 10000

# Run as the non-root user shipped with the official .NET images (UID 1654)
USER $APP_UID

ENTRYPOINT ["dotnet", "BackendAwSmartstay.API.dll"]
