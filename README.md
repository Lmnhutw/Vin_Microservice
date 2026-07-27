# Vin E-commerce Microservices

Vin is a .NET 8 e-commerce solution with separate APIs for authentication, products, coupons, shopping carts, and email notifications, plus an Angular client hosted by ASP.NET Core.

## Current architecture

- `Vin.Services.AuthAPI`: ASP.NET Identity, registration/login, roles, and JWT issuing.
- `Vin.Services.ProductAPI`: product CRUD backed by SQL Server.
- `Vin.Services.CouponAPI`: coupon CRUD backed by SQL Server.
- `Vin.Services.ShoppingCartAPI`: cart operations, synchronous calls to Product/Coupon APIs, and optional Azure Service Bus publishing.
- `Vin.Services.EmailAPI`: consumes cart messages when Service Bus is configured and persists email notification logs to SQL Server.
- `Vin.Services.OrderAPI`: owns order state and the checkout saga; it snapshots product prices and writes integration events through its outbox.
- `Vin.Services.PaymentAPI`: deterministic payment simulator that authorizes/rejects/refunds without claiming to process real money.
- `Vin.Services.InventoryAPI`: sole owner of configured stock and order reservations.
- `Vin.MessageBus`: shared Azure Service Bus publisher infrastructure.
- `Vin.Contracts`: versioned, data-only integration event contracts.
- `Vin.Eventing`: transactional outbox, idempotent inbox, and Azure Service Bus consumer/publisher building blocks.
- `VinWeb`: ASP.NET Core host and same-origin BFF that serves Angular and calls the internal APIs through typed services.

Each API owns its EF Core `DbContext` and database migrations. The local Compose stack includes the Azure Service Bus emulator and declarative topic/subscription setup so the Order/Payment/Inventory workflow can be inspected without a cloud subscription. Read [the architecture lab guide](docs/ARCHITECTURE.md) before changing the distributed flow.
## FE flow and API contract

The browser uses the same-origin Angular client and BFF. Internal service URLs and JWTs stay server-side:

~~~text
Angular component
  -> VinWeb BFF
  -> VinWeb Service
  -> BaseService.SendAsync
  -> API ResponseDTO
~~~

The implemented user flows are:

- Authentication: register, login, forgot password, and reset password.
- Catalog: product listing, details, create, update, and delete.
- Cart: load cart, add/update item, remove item, apply/remove coupon, and email cart.
- Administration: product and coupon write operations are protected by API roles.

The Cart API now enforces the authenticated JWT subject as the cart owner. It does not trust a caller-provided UserId or a route user ID, returns 403 for cross-user access, and validates missing cart headers/items before persistence. VinWeb/Service/BaseService.cs also accepts JSON-compatible response media types and evaluates HTTP error status before applying the successful-response content-type guard.

The FE and BE should be treated as one contract boundary: keep route names, HTTP methods, DTO property names, ResponseDTO, and 400/401/403/404/500 behavior synchronized when changing either side.

The Angular source is in `VinWeb/ClientApp`. Run `npm install` and `npm run build` from that directory for a production bundle. Docker builds the client automatically and copies it into the web host. Set `Frontend:UseAngular` to `false` only for a temporary Razor fallback.

## Local configuration

Secrets and connection strings are intentionally not stored in tracked `appsettings.json` files. Configure them with User Secrets or environment variables.

Example for AuthAPI:

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=Vin_Auth;Trusted_Connection=True;TrustServerCertificate=True" --project Vin.Services.AuthAPI
dotnet user-secrets set "ApiSettings:JwtOptions:Secret" "<generated-secret>" --project Vin.Services.AuthAPI
dotnet user-secrets set "ApiSettings:JwtOptions:Issuer" "vin-auth-api" --project Vin.Services.AuthAPI
dotnet user-secrets set "ApiSettings:JwtOptions:Audience" "vin-client" --project Vin.Services.AuthAPI
```

For ProductAPI and CouponAPI, use `ApiSettings:Secret`, `ApiSettings:Issuer`, and `ApiSettings:Audience`. For ShoppingCartAPI and EmailAPI, also configure:

```powershell
dotnet user-secrets set "ServiceBus:ConnectionString" "<service-bus-connection-string>" --project Vin.Services.ShoppingCartAPI
dotnet user-secrets set "ServiceBus:ConnectionString" "<service-bus-connection-string>" --project Vin.Services.EmailAPI
```

When the Azure Service Bus setting is absent, the APIs can start in local/degraded mode. Publishing and the EmailAPI consumer remain disabled until the setting is provided.

## Run and verify

### Docker Compose (recommended for local development)

With Docker Desktop running, start the complete stack from the repository root:

```powershell
docker compose up --build
```

The MVC web client is available at `http://localhost:8080`. The APIs are also
published for Swagger/debugging: Product `8081`, Auth `8082`, Coupon `8083`,
 Shopping Cart `8084`, Email `8085`, Order `8086`, Payment `8087`, and Inventory `8088` (for example,
`http://localhost:8081/swagger`). SQL Server is published at `localhost:1433`
and its data is persisted in the `sqlserver-data` Docker volume.

The Compose file provides development-only defaults for the SQL password and
JWT secret so the command works on a fresh machine. To override them, copy
`.env.example` to `.env` and set strong values there; `.env` is ignored by Git.
Compose supplies a development-only Service Bus emulator connection. It declares `vin-events` subscriptions and the `emailshoppingcart` queue in `infra/servicebus/Config.json`; the emulator is for local development/test only.

Stop the stack with `docker compose down`. Use
`docker compose down --volumes` only when you intentionally want to delete all
local SQL Server data.

### Run without Docker

```powershell
dotnet restore Vin.sln
dotnet build Vin.sln
dotnet run --project Vin.Services.AuthAPI
dotnet run --project Vin.Services.ProductAPI
dotnet run --project Vin.Services.CouponAPI
dotnet run --project Vin.Services.ShoppingCartAPI
dotnet run --project Vin.Services.EmailAPI
dotnet run --project Vin.Services.OrderAPI
dotnet run --project Vin.Services.PaymentAPI
dotnet run --project Vin.Services.InventoryAPI
dotnet run --project VinWeb
```

The APIs expose Swagger in Development. Database migrations are applied by each API at startup when its SQL Server connection is configured.
For a solution-level verification pass:

~~~powershell
dotnet build Vin.sln --no-restore
git diff --check
node .gitnexus/run.cjs analyze
~~~

GitNexus currently reports that its LadybugDB FTS extension is unavailable; graph indexing still succeeds, but full-text/BM25 search remains disabled until the extension is installed.

## Reliability and security notes

- Rotate any JWT secret that was previously committed to Git.
- Keep SQL Server, Service Bus, SendGrid, and other provider credentials in User Secrets or a managed secret store.
- Message Bus logs metadata only; it does not log connection-string fragments or message bodies.
- EmailAPI uses an `IHostedService` for Service Bus lifecycle and an `IDbContextFactory` for background database work.
- Invalid email-cart messages are dead-lettered; processing failures are rethrown so Service Bus can retry them.
- The current EmailAPI persists notification logs. A real email delivery provider can be added later without changing the message consumer contract.
- Checkout is at-least-once and eventually consistent. Each new workflow service stores integration events in a local outbox and processed event ids in a local inbox; Order owns saga state and requests payment compensation after an inventory rejection.
- `/health/live` proves a process is running; `/health/ready` checks its local database. `X-Correlation-ID`, `ActivitySource`, and `Meter` make cross-service tracing visible.
- Cart endpoints require JWT authentication and scope cart reads/writes to the authenticated user.
- BaseService.SendAsync is a shared FE boundary used by authentication, product, coupon, and cart flows; changes to it require full-solution verification.

## Future work

1. Add FE-BE integration tests for checkout success, payment rejection, inventory compensation, duplicate events, and DLQ inspection.
2. Attach an OpenTelemetry/OTLP exporter and local collector once a telemetry backend is selected.
3. Add a real email delivery adapter and provider-specific delivery status tracking.
4. Revisit cloud deployment and managed secret storage when the subscription is available.
