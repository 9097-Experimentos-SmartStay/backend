FROM mcr.microsoft.com/dotnet/sdk:9.0 AS builder
WORKDIR /app
COPY BackendAwSmartstay.API/*.csproj BackendAwSmartstay.API/
RUN dotnet restore ./BackendAwSmartstay.API
COPY . .
RUN dotnet publish ./BackendAwSmartstay.API -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=builder /app/out .
EXPOSE 80
ENTRYPOINT ["dotnet", "BackendAwSmartstay.API.dll"]

