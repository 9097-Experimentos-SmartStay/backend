FROM mcr.microsoft.com/dotnet/sdk:9.0 AS builder

WORKDIR /src

COPY BackendAwSmartstay.API/*.csproj BackendAwSmartstay.API/
RUN dotnet restore BackendAwSmartstay.API/BackendAwSmartstay.API.csproj

COPY . .

RUN dotnet publish BackendAwSmartstay.API/BackendAwSmartstay.API.csproj \
    -c Release \
    -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0

WORKDIR /app

COPY --from=builder /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "BackendAwSmartstay.API.dll"]
