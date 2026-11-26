# QuickBasket - Frontend

React 19 + TypeScript + Vite + Tailwind CSS v4 client for **QuickBasket** ("Groceries. Essentials. Delivered."), wired to the ASP.NET Core API in [`../backend`](../backend/README.md).

## Run

```bash
cd frontend
cp .env.example .env      # VITE_API_BASE_URL=https://localhost:7080
npm install
npm run dev               # http://localhost:5173  (start the backend first)
npm run build             # type-check + production build
```

The API must allow the origin `http://localhost:5173` (CORS). In development, trust the .NET dev certificate: `dotnet dev-certs https --trust`.

## What is implemented
- **Customer:** home, category, search, product detail, cart, checkout (address, slot, payment method), explicit "Confirm payment" dialog, Razorpay checkout page, order-specific UPI QR page (exact amount, expiry timer, polling, retry), order confirmation, tracking timeline, order history, saved addresses, scripted shopping assistant.
- **Admin** (`/admin/login`): overview, orders (status + partner assignment), products (add, price/stock, active), payments (details, refund with Idempotency-Key).
- **Delivery partner** (`/partner/login`): overview, active deliveries (pick up, deliver, cash-collected prompt), history.
- **Role locking:** each sign-in screen accepts only its own role and every area is behind `ProtectedRoute`. The server enforces roles too.

## Payment rules (enforced in the UI)
- The browser never sends an amount; the server computes it from the order.
- "Paid" is shown only when the server reports `Captured` (polling or webhook-confirmed), never from a browser callback or screenshot.
- Card/bank details are entered inside Razorpay's window; nothing sensitive is stored here. Only the public key id reaches the browser.
- The assistant can fill the cart only. It cannot start or approve a payment.
- "Simulate paid/failed" buttons appear only in `npm run dev` and only when the backend uses the Mock provider.

## Testing without Razorpay
Run the backend with `Payments:Provider=Mock` (the Development default), place an order, and use the dev simulate buttons on the payment screen.

## Layout
See [`docs/STRUCTURE.md`](docs/STRUCTURE.md). Only `src/services/*` talk to the API.

## Known limits
- Product photos are emoji tiles until the backend stores images.
- Admin overview numbers are computed in the browser from the latest 100 orders (no stats endpoint yet).
- Built and type-checked, and smoke-tested against a mocked API; not yet run against the real backend (which has not been compiled yet).
