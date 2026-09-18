# SmartStay — Backend API

API REST de SmartStay (ASP.NET Core 9, MySQL 8). Documentación interactiva en `/scalar` una vez que la API está en marcha.

## Configuración y secretos

La API lee su configuración por capas; cada capa sobrescribe a la anterior:

1. `appsettings.json`: valores por defecto, **sin secretos**.
2. `appsettings.{Environment}.json`: ajustes del entorno (`Development`, `Production`). `appsettings.Development.json` solo trae valores locales de prueba.
3. **User secrets** (solo en `Development`): secretos de tu máquina, fuera del repositorio.
4. **Variables de entorno**: lo que usan Docker Compose y Render. `__` (doble guion bajo) equivale a `:` (por ejemplo `TokenSettings__Secret` es `TokenSettings:Secret`).

Las opciones se validan al iniciar: si falta un valor obligatorio (por ejemplo `TokenSettings__Secret` o `Email__Smtp__Host` en producción), la API no arranca y el log indica qué variable falta.

Los medios de pago de las reservas (Yape, Plin, cuenta bancaria) **no** son variables de entorno: cada administrador los registra para su hotel desde la aplicación (`PUT /api/v1/hotels/{id}/payment-settings`). Un hotel sin medios de pago no acepta reservas.

### Desarrollo local con `dotnet run`

Usa `dotnet user-secrets` (el proyecto ya tiene un `UserSecretsId`):

```bash
cd BackendAwSmartstay.API
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "server=localhost;port=3306;user=smartstay;password=smartstay_dev;database=smartstay;"
dotnet user-secrets set "InitialChainAdmin:Email" "admin@tu-dominio.com"
dotnet user-secrets set "InitialChainAdmin:Password" "una frase larga para el administrador"
dotnet user-secrets list
dotnet run
```

Los user secrets solo se cargan cuando `ASPNETCORE_ENVIRONMENT=Development` (el comportamiento por defecto de ASP.NET Core). Sin SMTP configurado, los correos se escriben en el log con sus enlaces y códigos.

### Docker Compose

`docker-compose.yml` levanta MySQL y la API. Para cambiar valores, copia `.env.example` a `.env` (está en `.gitignore`): **ese archivo lo lee Docker Compose, no la aplicación**, y Compose pasa sus valores al contenedor como variables de entorno.

```bash
cp .env.example .env   # opcional
docker compose up -d --build
curl http://localhost:10000/health
```

### Producción (Render)

Define cada valor como **variable de entorno** del servicio en Render (nunca en el repositorio). Los archivos sensibles, como el certificado CA de Aiven para MySQL, se suben como **Secret Files** (quedan en `/etc/secrets/`) y se referencian desde la variable, por ejemplo `SslMode=VerifyFull;SslCa=/etc/secrets/ca.pem;` en la cadena de conexión. `.env.example` lista todas las variables con su explicación.

Las tareas programadas (`/demo-requests/follow-ups`, `/bookings/expire-pending`, `/rooms/maintenance-alerts`) las invoca un programador externo (por ejemplo, un cron de GitHub Actions) con la cabecera `X-Cron-Key`.
