# QuickBasket - Backend

ASP.NET Core Web API (.NET 10). Solution file: `QuickBasket.slnx`.

## Projects

| Project | Role | Depends on |
|---|---|---|
| `QuickBasket.Domain` | Entities and enums | nothing |
| `QuickBasket.Payments` | **Standalone payment module**: `IPaymentProvider`, Razorpay provider, mock provider, signature helpers | nothing (swap providers here) |
| `QuickBasket.Application` | DTOs, service and repository interfaces, business logic, payment state machine | Domain, Payments |
| `QuickBasket.Infrastructure` | EF Core (SQL Server), repositories, JWT/BCrypt, seeder, payment-expiry worker | Application, Domain, Payments |
| `QuickBasket.Api` | Controllers, middleware, auth policies, Program.cs | all of the above |
| `QuickBasket.Tests` | xUnit tests for the payment module and state machine | Domain, Payments, Application |

## API surface

| Area | Endpoints |
|---|---|
| Auth | `POST /api/auth/register`, `POST /api/auth/login`, `GET /api/auth/me` |
| Catalogue | `GET /api/categories`, `GET /api/products`, `GET /api/products/{id}`; admin: `POST /api/admin/categories`, `POST/PUT /api/admin/products` |
| Cart / addresses | `/api/cart`, `/api/cart/items`, `/api/addresses` (Customer) |
| Orders | `POST /api/orders/checkout`, `GET /api/orders`, `GET /api/orders/{id}`, `POST /api/orders/{id}/cancel` |
| Payments | `POST /api/payments/initiate` (Idempotency-Key), `POST /api/payments/verify`, `GET /api/payments/{id}`, `GET /api/payments/order/{orderId}` |
| Webhook | `POST /api/webhooks/razorpay` (raw body, signature verified) |
| Admin | orders list/status/assign, payments list/detail, `POST /api/admin/payments/{id}/refunds` (Idempotency-Key), partners |
| Partner | `GET /api/partner/orders`, `POST .../pickup`, `POST .../deliver` |

## Payment design (summary)
1. Server recalculates the order total from the database; the client never sends an amount.
2. `initiate` requires `confirmedByCustomer: true` and a customer token with the `payments` scope (a future AI-assistant token will not have it).
3. Razorpay order or UPI QR is created server-side with secrets from configuration.
4. Browser result is verified by signature **and** re-fetched from the gateway; the webhook (raw-body HMAC) is the second path. Both use `PaymentStateUpdater`.
5. Duplicate webhooks are rejected by a unique `(Provider, EventId)` index; duplicate initiations by a unique `(UserId, IdempotencyKey)` index; double settlement by a `RowVersion` concurrency token.
6. Live Razorpay keys are rejected unless `Payments:Razorpay:AllowLive=true`.

## Configuration
Never commit secrets. Use environment variables, `dotnet user-secrets`, or Azure Key Vault. See `.env.example` for key names
(`Payments__Razorpay__KeySecret`, `Jwt__SigningKey`, ...). Development defaults (`appsettings.Development.json`) use the **Mock** provider,
LocalDB, and clearly labelled dev-only seed credentials.

## Run (after installing the .NET 10 SDK)
```bash
cd backend
dotnet restore
dotnet build
dotnet test
dotnet run --project src/QuickBasket.Api     # https://localhost:7080
```

## How to test the backend

**1. Automated tests**
```bash
dotnet test
```

**2. Swagger UI (interactive, Development only)**: open `https://localhost:7080/swagger`
1. `POST /api/auth/login` with a seeded user, e.g. `{"email":"admin@quickbasket.local","password":"DevAdmin#12345"}`.
2. Copy `accessToken`, click **Authorize**, paste it (without the word "Bearer").
3. Try the protected endpoints. Customers: register via `POST /api/auth/register`.
The raw document is at `/openapi/v1.json`.

**3. Full flow without a payment gateway**: `requests/quickbasket.http` runs register, cart, checkout, payment, refund and role checks in order
(VS Code "REST Client" extension, Visual Studio or Rider).

**4. Paying with the mock provider (Development only)**
After `POST /api/payments/initiate`, call
`POST /api/dev/mock-payments/{paymentId}/simulate?success=true` (or `false`). It acts as the gateway and sends a correctly signed webhook
through the same code path Razorpay uses, so the payment becomes Captured and the order Confirmed. Refunds finish with
`POST /api/dev/mock-payments/refunds/{refundId}/process`. These endpoints return 404 unless the environment is Development and `Payments:Provider` is `Mock`.

**5. Razorpay test mode**: see `PROJECT_PROGRESS.md` section 5B.

Seeded dev users (`appsettings.Development.json`): `admin@quickbasket.local` / `DevAdmin#12345`, `partner@quickbasket.local` / `DevPartner#12345`.
