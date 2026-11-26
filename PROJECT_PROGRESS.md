# QuickBasket - Project Progress

Last updated: 2026-11-22 (end of day 1 build session)

> **Important:** the .NET SDK was not available in the build environment (install blocked by network policy), so the backend
> has **never been compiled or run**. Package versions were chosen from memory and are **unverified**. The first thing to do
> tomorrow is `dotnet restore && dotnet build` and fix whatever the compiler finds. Only a brace-balance check was run.
>
> Token usage could not be measured from inside the session, so this file was written at the point where the planned
> backend and frontend-structure scope was complete, not at an exact 50% reading.

## 1. Backend files created and implemented

Root: `backend/` (`QuickBasket.slnx`, `Directory.Build.props` targeting net10.0, `.gitignore`, `.env.example`, `README.md`)

**QuickBasket.Domain** - entities `User, Address, Category, Product, Cart/CartItem, Order/OrderItem/OrderEvent, Payment, PaymentTransaction, PaymentWebhookEvent, Refund`; enums for roles, order/payment/refund/webhook statuses and payment methods.

**QuickBasket.Payments** (standalone, swappable) - `IPaymentProvider`, `RazorpayPaymentProvider` (REST, test-key guard), `MockPaymentProvider` (tests/local), `RazorpayWebhookParser`, `SignatureHelper` (HMAC-SHA256 + constant-time compare), `Money` (paise conversion), `AddPaymentModule` DI switch (`Payments:Provider` = Mock | Razorpay).

**QuickBasket.Application** - DTOs, repository/service interfaces, services: `AuthService, CatalogService, CartService, AddressService, OrderService, PaymentService, PaymentWebhookService, RefundService`, plus `PaymentStateUpdater` (single place that changes payment/order state), `ConcurrencyRetry`, `WebhookPayloadSanitizer`, options classes.

**QuickBasket.Infrastructure** - `QuickBasketDbContext` + 4 configuration files (unique/idempotency indexes, rowversion), repositories, `UnitOfWork` (transactions, concurrency and duplicate-key mapping), `JwtTokenService`, `BcryptPasswordHasher`, `DbSeeder`, `PaymentExpiryWorker`, DI extension.

**QuickBasket.Api** - `Program.cs` (JWT, policies Admin/Customer/Partner/PaymentActions, CORS, rate limiting, OpenAPI, health), exception middleware, controllers: `Auth, Catalog, Cart, Addresses, Orders, Payments, PaymentWebhooks, Admin, Partner`, `appsettings*.json`, `launchSettings.json`.

**QuickBasket.Tests** - xUnit: payment providers/signatures/money, `PaymentStateUpdater` state machine (duplicates, mismatch, out-of-order, refunds), webhook sanitizer.

Full list:

```
backend/.env.example
backend/.gitignore
backend/Directory.Build.props
backend/QuickBasket.slnx
backend/README.md
backend/src/QuickBasket.Api/Controllers/AddressesController.cs
backend/src/QuickBasket.Api/Controllers/AdminController.cs
backend/src/QuickBasket.Api/Controllers/AuthController.cs
backend/src/QuickBasket.Api/Controllers/CartController.cs
backend/src/QuickBasket.Api/Controllers/CatalogController.cs
backend/src/QuickBasket.Api/Controllers/OrdersController.cs
backend/src/QuickBasket.Api/Controllers/PartnerController.cs
backend/src/QuickBasket.Api/Controllers/PaymentWebhooksController.cs
backend/src/QuickBasket.Api/Controllers/PaymentsController.cs
backend/src/QuickBasket.Api/Extensions/ClaimsPrincipalExtensions.cs
backend/src/QuickBasket.Api/Middleware/ExceptionHandlingMiddleware.cs
backend/src/QuickBasket.Api/Program.cs
backend/src/QuickBasket.Api/Properties/launchSettings.json
backend/src/QuickBasket.Api/QuickBasket.Api.csproj
backend/src/QuickBasket.Api/appsettings.Development.json
backend/src/QuickBasket.Api/appsettings.json
backend/src/QuickBasket.Application/Abstractions/Repositories/Repositories.cs
backend/src/QuickBasket.Application/Abstractions/Services/Security.cs
backend/src/QuickBasket.Application/Abstractions/Services/ServiceInterfaces.cs
backend/src/QuickBasket.Application/ApplicationServiceExtensions.cs
backend/src/QuickBasket.Application/Common/Exceptions.cs
backend/src/QuickBasket.Application/Common/Options.cs
backend/src/QuickBasket.Application/Common/PagedResult.cs
backend/src/QuickBasket.Application/DTOs/AuthDtos.cs
backend/src/QuickBasket.Application/DTOs/CartDtos.cs
backend/src/QuickBasket.Application/DTOs/CatalogDtos.cs
backend/src/QuickBasket.Application/DTOs/OrderDtos.cs
backend/src/QuickBasket.Application/DTOs/PaymentDtos.cs
backend/src/QuickBasket.Application/Payments/ConcurrencyRetry.cs
backend/src/QuickBasket.Application/Payments/PaymentStateUpdater.cs
backend/src/QuickBasket.Application/Payments/WebhookPayloadSanitizer.cs
backend/src/QuickBasket.Application/QuickBasket.Application.csproj
backend/src/QuickBasket.Application/Services/AddressService.cs
backend/src/QuickBasket.Application/Services/AuthService.cs
backend/src/QuickBasket.Application/Services/CartService.cs
backend/src/QuickBasket.Application/Services/CatalogService.cs
backend/src/QuickBasket.Application/Services/Mappers.cs
backend/src/QuickBasket.Application/Services/OrderService.cs
backend/src/QuickBasket.Application/Services/PaymentService.cs
backend/src/QuickBasket.Application/Services/PaymentWebhookService.cs
backend/src/QuickBasket.Application/Services/RefundService.cs
backend/src/QuickBasket.Domain/Common/BaseEntity.cs
backend/src/QuickBasket.Domain/Entities/Address.cs
backend/src/QuickBasket.Domain/Entities/Cart.cs
backend/src/QuickBasket.Domain/Entities/Category.cs
backend/src/QuickBasket.Domain/Entities/Order.cs
backend/src/QuickBasket.Domain/Entities/Payment.cs
backend/src/QuickBasket.Domain/Entities/Product.cs
backend/src/QuickBasket.Domain/Entities/User.cs
backend/src/QuickBasket.Domain/Enums/Enums.cs
backend/src/QuickBasket.Domain/QuickBasket.Domain.csproj
backend/src/QuickBasket.Infrastructure/Auth/AuthServices.cs
backend/src/QuickBasket.Infrastructure/InfrastructureServiceExtensions.cs
backend/src/QuickBasket.Infrastructure/Persistence/Configurations/IdentityAndCatalogConfigurations.cs
backend/src/QuickBasket.Infrastructure/Persistence/Configurations/OrderConfigurations.cs
backend/src/QuickBasket.Infrastructure/Persistence/Configurations/PaymentConfigurations.cs
backend/src/QuickBasket.Infrastructure/Persistence/DbSeeder.cs
backend/src/QuickBasket.Infrastructure/Persistence/QuickBasketDbContext.cs
backend/src/QuickBasket.Infrastructure/Persistence/UnitOfWork.cs
backend/src/QuickBasket.Infrastructure/QuickBasket.Infrastructure.csproj
backend/src/QuickBasket.Infrastructure/Repositories/Repositories.cs
backend/src/QuickBasket.Infrastructure/Workers/PaymentExpiryWorker.cs
backend/src/QuickBasket.Payments/Abstractions/IPaymentProvider.cs
backend/src/QuickBasket.Payments/Mock/MockPaymentProvider.cs
backend/src/QuickBasket.Payments/Models/GatewayModels.cs
backend/src/QuickBasket.Payments/Models/Money.cs
backend/src/QuickBasket.Payments/Models/PaymentExceptions.cs
backend/src/QuickBasket.Payments/PaymentModuleExtensions.cs
backend/src/QuickBasket.Payments/QuickBasket.Payments.csproj
backend/src/QuickBasket.Payments/Razorpay/RazorpayOptions.cs
backend/src/QuickBasket.Payments/Razorpay/RazorpayPaymentProvider.cs
backend/src/QuickBasket.Payments/Razorpay/RazorpayWebhookParser.cs
backend/src/QuickBasket.Payments/Security/SignatureHelper.cs
backend/tests/QuickBasket.Tests/PaymentProviderTests.cs
backend/tests/QuickBasket.Tests/PaymentStateUpdaterTests.cs
backend/tests/QuickBasket.Tests/QuickBasket.Tests.csproj
```

## 2. Frontend folders and placeholder files created

`frontend/` contains **103 placeholder files in 28 folders** under `src/` (+ `public/`, `docs/`). Each placeholder is a 2-line comment stating its purpose; nothing is implemented. No dependencies installed, no npm/Vite commands run.

Folders: `styles, config, routes, layouts, pages/{auth,customer,admin,partner}, components/{ui,layout,product,cart,checkout,payment,tracking,assistant,admin,partner}, services, hooks, contexts, types, utils`.

Config files (declared only): `package.json` (React 19, react-router, Vite, Tailwind 4, TS), `vite.config.ts`, `tsconfig.json`, `index.html`, `.env.example`, `.gitignore`.

Docs: `frontend/README.md` and `frontend/docs/STRUCTURE.md` (a table of every folder and file with its purpose).

## 3. Backend tasks

**Completed (written, unverified)**
- [x] Solution structure and layering; separate payment module behind `IPaymentProvider`
- [x] Entities and EF configuration for all payment tables; indexes for idempotency, webhook de-duplication, concurrency
- [x] JWT auth, roles, `PaymentActions` scope policy (AI-assistant tokens must not carry it), BCrypt hashing, rate limiting on auth/payments
- [x] Catalogue, cart, addresses, order checkout with server-side pricing and stock reservation, cancellation, status transitions, partner assignment and delivery (COD collection)
- [x] Payment initiate (UPI intent/card/netbanking via Razorpay order, dynamic UPI QR, COD), explicit-confirmation flag, Idempotency-Key replay, retry after failure/expiry
- [x] Checkout verification (signature + gateway re-fetch), raw-body webhook verification, duplicate-event handling, failure/expiry/refund events, payment expiry worker
- [x] Refunds with idempotency and over-refund protection
- [x] Live Razorpay keys blocked unless `AllowLive=true`; secrets only from configuration; webhook payload sanitised before storage
- [x] Unit tests for the payment module and state machine
- [x] Swagger UI at `/swagger` (Development) with a JWT Authorize button, `backend/requests/quickbasket.http` end-to-end file, and dev-only mock payment simulator (`/api/dev/mock-payments/...`)

**Pending**
- [ ] **Install .NET 10 SDK, restore, build, fix compile errors; verify NuGet versions**
- [ ] Run tests; fix failures
- [ ] Generate the EF migration (`InitialCreate`); dev currently uses `EnsureCreated`
- [ ] Integration tests (WebApplicationFactory + SQL Server/Testcontainers): checkout -> initiate -> webhook -> confirmed; duplicate webhook; concurrent settle
- [ ] Service-level tests for `PaymentService`, `PaymentWebhookService`, `RefundService`, `OrderService`
- [ ] Test against real Razorpay **test mode** (create test keys, expose webhook via tunnel, confirm event names/payload shapes, QR `close_by` rules)
- [ ] Auto-cancel unpaid orders after a timeout and restore stock (worker currently only expires payment attempts)
- [ ] Admin dashboard aggregate endpoints (revenue, orders today, low stock); partner availability toggle/earnings
- [ ] Pagination/sorting on partner and customer order lists; search improvements
- [ ] Structured logging, correlation ids, audit log for admin actions
- [ ] Key Vault configuration provider; production CORS and HSTS review
- [ ] AI assistant endpoints (shopping lists only, no payment access) - future
- [ ] Docker, Azure deployment, CI/CD, GitHub repository setup (deliberately not started)
- [ ] Do not enable live payments until business account and gateway onboarding are approved

## 4. Frontend status

**Implemented (2026-11-26):** all roadmap items below are built in `frontend/` (customer, admin, partner, payments UI, assistant). `npm run build` passes and a mocked-API browser test passed (login role locks, cart, checkout, UPI QR, simulate paid, confirmation). Still to do: run against the real backend once it compiles; confirm CORS allows `http://localhost:5173`. Original roadmap kept for reference:

1. `npm install`; confirm Vite + React 19 + Tailwind 4 dev server starts; adjust versions in `package.json` if needed.
2. Foundation: `config/env.ts`, `services/apiClient.ts` (bearer token, problem+json errors), `types/*` mirroring backend DTOs, `styles/index.css` brand tokens, `utils/*`.
3. Contexts and routing: `AuthContext`, `CartContext`, `ToastContext`, `routes/*` with `ProtectedRoute`, the four layouts.
4. Auth pages, then catalogue: Home, Category, Search, ProductDetail with product components.
5. Cart and addresses, then Checkout (server-calculated totals only).
6. **Payments**: `ConfirmPaymentDialog` (sends `confirmedByCustomer: true`), `PaymentPage` with Razorpay checkout using only the public key id, `UpiQrPaymentPage` (QR image, exact amount, expiry timer, polling via `usePaymentPolling`, retry), COD path. Success UI only after backend status is Captured/Confirmed.
7. Order confirmation, tracking timeline, order history.
8. Shopping assistant panel (scripted/mock first; may add to cart, never touches payments).
9. Admin pages (dashboard, orders, products, payments + refund) and partner pages (dashboard, active delivery, COD prompt, history).
10. Responsive polish, accessibility pass, browser check at phone and desktop widths.

## 5. Exact commands and instructions to resume

**A. Backend first (before any frontend work)**
```bash
# 1. Install the .NET 10 SDK (https://dotnet.microsoft.com/download), then:
cd QuickBasket/backend
dotnet --list-sdks                 # expect 10.x; if only 9.x, change TargetFramework in Directory.Build.props and the 10.0.0 package versions
dotnet restore
dotnet build                       # fix compile errors / wrong package versions first
dotnet test

# 2. Local settings (dev defaults already use the Mock payment provider + LocalDB)
dotnet user-secrets init --project src/QuickBasket.Api
dotnet user-secrets set "Jwt:SigningKey" "<random 32+ chars>" --project src/QuickBasket.Api   # optional; a DEV-ONLY key exists in appsettings.Development.json

# 3. Run
dotnet run --project src/QuickBasket.Api       # https://localhost:7080  -> open https://localhost:7080/swagger
# dev seed users (appsettings.Development.json): admin@quickbasket.local, partner@quickbasket.local

# 4. Create the real migration, then set Database:ApplyMigrations=true and Database:EnsureCreated=false
dotnet tool install --global dotnet-ef
dotnet ef migrations add InitialCreate --project src/QuickBasket.Infrastructure --startup-project src/QuickBasket.Api --output-dir Persistence/Migrations
```

**B. Razorpay test mode (only after the build is green)**
```bash
dotnet user-secrets set "Payments:Provider" "Razorpay" --project src/QuickBasket.Api
dotnet user-secrets set "Payments:Razorpay:KeyId" "rzp_test_..." --project src/QuickBasket.Api
dotnet user-secrets set "Payments:Razorpay:KeySecret" "<test secret>" --project src/QuickBasket.Api
dotnet user-secrets set "Payments:Razorpay:WebhookSecret" "<test webhook secret>" --project src/QuickBasket.Api
# Webhook URL for the Razorpay dashboard (test mode): https://<tunnel-host>/api/webhooks/razorpay
# Events to enable: payment.authorized, payment.captured, payment.failed, qr_code.credited, qr_code.closed, refund.processed, refund.failed
```

**C. Frontend (after A passes)**
```bash
cd QuickBasket/frontend
cp .env.example .env
npm install
npm run dev                        # http://localhost:5173
```

**Rules to keep:** do not delete files from the agreed structure; replace any remaining placeholder's comment with the real implementation when you reach it. Keep backend and frontend in their own folders. Never commit secrets or `.env`. Test mode only.
