# SmartStay — Backend API

API REST de SmartStay (ASP.NET Core 9, MySQL 8). Documentación interactiva en `/scalar` una vez que la API está en marcha.

## Configuración y secretos

La API lee su configuración por capas; cada capa sobrescribe a la anterior:

1. `appsettings.json`: valores por defecto, **sin secretos**.
2. `appsettings.{Environment}.json`: ajustes del entorno (`Development`, `Production`). `appsettings.Development.json` solo trae valores locales de prueba.
3. **User secrets** (solo en `Development`): secretos de tu máquina, fuera del repositorio.
4. **Variables de entorno**: lo que usan Docker Compose y Render. `__` (doble guion bajo) equivale a `:` (por ejemplo `TokenSettings__Secret` es `TokenSettings:Secret`).

Las opciones se validan al iniciar: si falta un valor obligatorio (por ejemplo `TokenSettings__Secret` o `Email__Brevo__ApiKey` en producción), la API no arranca y el log indica qué variable falta.

Las imágenes de los hoteles se suben directo del navegador a Cloudinary con una **firma de corta vida** que emite la API (`POST /api/v1/media/hotel-images/signature`, solo administradores); el API secret vive solo en el servidor. Define `Cloudinary__CloudName`, `Cloudinary__ApiKey` y `Cloudinary__ApiSecret` (obligatorias en producción; sin ellas, en desarrollo la firma responde 503 `media.uploads_not_configured`). El upload preset firmado es `smartstay-hotels` (`Cloudinary__HotelImagesPreset`).

**Correos.** El transporte se elige de forma explícita con `Email__Transport`:

- `BrevoApi` (producción): API HTTP transaccional de Brevo (`POST https://api.brevo.com/v3/smtp/email`) por el puerto 443. Requiere `Email__Brevo__ApiKey` (secreto) y `Email__From__Address` (remitente verificado en Brevo). Se usa la API y no SMTP porque desde Render las conexiones salientes a `smtp-relay.brevo.com:587` fallan por timeout.
- `Smtp`: cualquier relay SMTP (`Email__Smtp__Host`, `Port`, `Username`, `Password`, `EnableSsl`), pensado para uso local (por ejemplo Mailpit).
- `Log`: el correo se escribe en el log con sus enlaces y códigos. Es el valor por defecto fuera de producción; en `Production` la API no arranca con `Log` ni sin transporte.

Cada correo se guarda en la tabla `outbox_emails` **en la misma transacción** que el cambio que lo origina (outbox transaccional): si el cambio se revierte, el correo no existe; si se confirma, el correo se entrega aunque la API se reinicie o se redespliegue. Un despachador en segundo plano entrega los pendientes cada `Email__Outbox__PollIntervalSeconds` (10 s), reintenta los fallos transitorios con backoff exponencial hasta `Email__Outbox__MaxAttempts` (8) y marca `Failed` los rechazos permanentes (remitente o destinatario inválido, API key incorrecta). En el log queda una línea por correo enviado con el destinatario enmascarado; nunca la API key ni el cuerpo.

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

Los user secrets solo se cargan cuando `ASPNETCORE_ENVIRONMENT=Development` (el comportamiento por defecto de ASP.NET Core). Sin `Email:Transport` (o con `Log`), los correos se escriben en el log con sus enlaces y códigos.

### Docker Compose

`docker-compose.yml` levanta MySQL y la API. Para cambiar valores, copia `.env.example` a `.env` (está en `.gitignore`): **ese archivo lo lee Docker Compose, no la aplicación**, y Compose pasa sus valores al contenedor como variables de entorno.

```bash
cp .env.example .env   # opcional
docker compose up -d --build
curl http://localhost:10000/health
```

### Producción (Render)

Define cada valor como **variable de entorno** del servicio en Render (nunca en el repositorio). Los archivos sensibles, como el certificado CA de Aiven para MySQL, se suben como **Secret Files** (quedan en `/etc/secrets/`) y se referencian desde la variable, por ejemplo `SslMode=VerifyFull;SslCa=/etc/secrets/ca.pem;` en la cadena de conexión. `.env.example` lista todas las variables con su explicación.

Las tareas programadas (`/demo-requests/follow-ups`, `/bookings/expire-pending`, `/rooms/maintenance-alerts`, `/emails/dispatch`) las invoca un programador externo (por ejemplo, un cron de GitHub Actions) con la cabecera `X-Cron-Key`. `/emails/dispatch` entrega los correos pendientes o en reintento (el plan free de Render duerme la API cuando no hay tráfico) y es idempotente.

**IP del cliente (rate limiting y auditoría).** En Render el tráfico llega por Cloudflare → balanceador interno de Render → Kestrel, así que la IP del socket es la del balanceador. La API procesa siempre `X-Forwarded-For` y `X-Forwarded-Proto` con `ForwardedHeadersOptions` configuradas en código (primer middleware del pipeline): recorre la cadena de derecha a izquierda saltando solo proxies de confianza (loopback, rangos privados `10/8`, `172.16/12`, `192.168/16`, `100.64/10`, `fc00::/7` y los rangos publicados de Cloudflare) y toma la primera IP que no es de confianza como cliente. Lo que un cliente escriba más a la izquierda se ignora, así que no puede falsear su IP. `ForwardedHeaders__TrustedNetworks__0`, `__1`... reemplazan la lista por defecto (CIDR). No definas `ASPNETCORE_FORWARDEDHEADERS_ENABLED`: confía en cualquier proxy pero solo desenvuelve un salto y devolvía la IP interna del balanceador para todos los usuarios. Si Cloudflare publica rangos nuevos (https://www.cloudflare.com/ips), actualiza `ForwardedHeadersSettings.DefaultTrustedNetworks`.

## Datos de demostración

Las migraciones solo crean el esquema y los catálogos de referencia que la aplicación necesita (categorías de hospedaje y amenidades del formulario de hotel). Una base de datos nueva **no** tiene hoteles, habitaciones ni cuentas. Los datos de demostración son opcionales y se cargan al iniciar, después de las migraciones, solo con `DemoData__Enabled=true`:

- Se crean **una sola vez**: si ya existe alguna cuenta u hotel de demostración, no se hace nada (reiniciar no duplica datos). Todo ocurre en una transacción.
- Pasan por el dominio (agregados, política de contraseñas, disponibilidad), no por SQL.
- **No se envía ningún correo**: los hechos se registran con su fecha en el pasado y sus eventos de dominio no se publican. Las cuentas quedan con el correo ya verificado; el staff igual debe activar su app de autenticación (MFA) en el primer inicio de sesión.
- Las fechas son relativas a "hoy" en `America/Lima`, así que el calendario siempre se ve con movimiento.

| Variable | Uso |
|---|---|
| `DemoData__Enabled` | `true` para cargar los datos (por defecto `false`). |
| `DemoData__DefaultPassword` | **Secreto.** Contraseña de todas las cuentas de demostración: mínimo 15 caracteres (política de huéspedes, también válida para el staff), no común ni filtrada. Sin ella la API no arranca si los datos están activados. |
| `DemoData__EmailBase` | Buzón base (por defecto `psulcasanchez@gmail.com`). Las cuentas usan sus alias de Gmail: `psulcasanchez+admin1@gmail.com`, etc. |
| `DemoData__Hotel1Payment__AccountHolder`, `__Yape`, `__Plin`, `__BankName`, `__BankAccountNumber`, `__BankAccountCci` | **Datos personales**: medios de pago del hotel 1 (se validan igual que el formulario de la app). Si no se definen, el hotel 1 queda sin medios de pago, no acepta reservas y no se crean las reservas de demostración (queda un aviso en el log). Nunca se inventan números. |

Qué se crea (las cuentas son `<buzón>+<alias>@<dominio>`):

| Alias | Rol | Qué muestra |
|---|---|---|
| `admin1` | admin de **Casa Ungurahui Hotel Boutique** (Tarapoto) | Hotel con 8 habitaciones (101–104, 201–204; Simple, Doble, Matrimonial, Suite; S/ 150–380), medios de pago configurados, staff, calendario y mapa de habitaciones. |
| `recepcion1` | reception del hotel 1 | Mapa de habitaciones (103 ocupada, 202 en limpieza, 204 en mantenimiento hace más de 24 h con alerta de vencida), historial de estados, calendario y reservas del hotel. Registró los pagos. |
| `limpieza1` | housekeeping del hotel 1 | Puso la 202 en limpieza. |
| `mantenimiento1` | maintenance del hotel 1 | Puso la 204 en mantenimiento. |
| `admin2` | admin de **Wayra Sacha Ecolodge** (Lamas) | Ecolodge con 5 habitaciones disponibles (bungalows B1–B2, M1–M2, D1), **sin medios de pago y sin staff**. |
| `huesped1` | guest (con perfil de huésped) | Estadía en curso en la 103 (pagada en efectivo en recepción) y una reserva cancelada por el huésped después de pagar (pago reembolsado). |
| `huesped2` | guest (con perfil de huésped) | Una reserva confirmada con pago por Yape registrado por recepción, una pendiente de pago (vence en menos de 24 h) y una vencida sin pago. |

Flujos que se hacen **en vivo** durante la demostración (no se precargan):

- Registro de un huésped nuevo y verificación de su correo (US-01).
- `admin2` crea su staff (US-03).
- Recuperación de contraseña (US-04).
- Activación de MFA del staff en su primer inicio de sesión (US-52).
- `admin2` configura los medios de pago del ecolodge (US-53).

Para volver a cargar los datos desde cero hay que partir de una base de datos vacía (en local: `docker compose down -v`).
