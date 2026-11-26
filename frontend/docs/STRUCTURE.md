# QuickBasket frontend - folder and file guide

Every file below is now implemented (see `../README.md`). `vite-env.d.ts`, `utils/assistantLogic.ts`, `utils/paymentSession.ts`, `utils/razorpay.ts`, `hooks/useInitiatedPayment.ts` and `hooks/usePartnerOrders.ts` were added during implementation.

## Folders

| Folder | Purpose |
|---|---|
| `src/` | All application source code. |
| `src/styles/` | Global CSS: Tailwind entry and QuickBasket brand tokens. |
| `src/config/` | Typed access to environment configuration (VITE_* variables only; never secrets). |
| `src/routes/` | Route table, route path constants and role-based route guards. |
| `src/layouts/` | Page shells shared by groups of pages (customer, admin, partner, auth). |
| `src/pages/` | One component per screen, grouped by audience. |
| `src/pages/auth/` | Login and registration screens. |
| `src/pages/customer/` | Customer shopping journey: browse, search, cart, checkout, payment, tracking. |
| `src/pages/admin/` | Admin dashboard screens. |
| `src/pages/partner/` | Delivery-partner dashboard screens. |
| `src/components/` | Reusable UI building blocks, grouped by feature. |
| `src/components/ui/` | Generic design-system primitives (button, badge, modal, toast...). |
| `src/components/layout/` | Header, bottom navigation, footer and the QuickBasket logo. |
| `src/components/product/` | Product browsing components. |
| `src/components/cart/` | Cart line items and bill summary. |
| `src/components/checkout/` | Checkout steps: address, slot, payment method, explicit confirmation. |
| `src/components/payment/` | Payment UI: UPI QR card, expiry timer, status banner, Razorpay checkout launcher. |
| `src/components/tracking/` | Order tracking timeline and delivery-partner card. |
| `src/components/assistant/` | QuickBasket shopping assistant chat UI (a feature inside QuickBasket). |
| `src/components/admin/` | Admin-only widgets: stat cards, orders table, refund dialog. |
| `src/components/partner/` | Delivery-partner widgets: task card, COD collection prompt. |
| `src/services/` | API layer. One module per backend area; the only place that calls fetch/HTTP. |
| `src/hooks/` | Reusable React hooks that wrap services and UI logic. |
| `src/contexts/` | React context providers for cross-cutting state (auth, cart, toasts, assistant). |
| `src/types/` | TypeScript types mirroring backend DTOs. |
| `src/utils/` | Pure helper functions and constants. |
| `public/` | Static assets served as-is (favicon, images). |
| `docs/` | Frontend documentation. |

## Files

| File | Purpose |
|---|---|
| `src/main.tsx` | App entry point: mounts <App/> and providers into #root. |
| `src/App.tsx` | Root component: context providers plus the router outlet. |
| `src/vite-env.d.ts` | Vite client type references and typed import.meta.env. |
| `src/styles/index.css` | Tailwind import and QuickBasket brand design tokens (colors, fonts, radii). |
| `src/config/env.ts` | Reads VITE_API_BASE_URL and other public settings with validation. |
| `src/routes/index.tsx` | Router definition mapping paths to pages and layouts. |
| `src/routes/routePaths.ts` | Central constants for every URL path. |
| `src/routes/ProtectedRoute.tsx` | Guard that redirects unauthenticated users and enforces Customer/Admin/Partner roles. |
| `src/layouts/CustomerLayout.tsx` | Header, bottom nav, cart badge and assistant launcher for customer pages. |
| `src/layouts/AdminLayout.tsx` | Sidebar + top bar for admin pages. |
| `src/layouts/PartnerLayout.tsx` | Mobile-first shell for delivery-partner pages. |
| `src/layouts/AuthLayout.tsx` | Centered card layout for login and registration. |
| `src/pages/NotFoundPage.tsx` | 404 screen. |
| `src/pages/auth/LoginPage.tsx` | Email and password sign-in. |
| `src/pages/auth/RegisterPage.tsx` | Customer sign-up. |
| `src/pages/customer/HomePage.tsx` | Hero with tagline 'Groceries. Essentials. Delivered.', category tiles, deals. |
| `src/pages/customer/CategoryPage.tsx` | Product listing for one category with filters and sorting. |
| `src/pages/customer/SearchPage.tsx` | Search results with suggestions and empty state. |
| `src/pages/customer/ProductDetailPage.tsx` | Single product view with add to cart and related items. |
| `src/pages/customer/CartPage.tsx` | Cart review with server-calculated totals. |
| `src/pages/customer/CheckoutPage.tsx` | Address, delivery slot and payment-method selection; creates the order. |
| `src/pages/customer/PaymentPage.tsx` | Explicit payment confirmation and Razorpay checkout launch (UPI intent, card, net banking, COD). |
| `src/pages/customer/UpiQrPaymentPage.tsx` | Order-specific UPI QR with exact amount, expiry countdown, status polling and retry. |
| `src/pages/customer/OrderConfirmationPage.tsx` | Shown only after the backend reports the order as confirmed. |
| `src/pages/customer/OrderTrackingPage.tsx` | Order status timeline and delivery-partner details. |
| `src/pages/customer/OrderHistoryPage.tsx` | Past orders with reorder action. |
| `src/pages/customer/AddressesPage.tsx` | Manage saved delivery addresses. |
| `src/pages/admin/AdminDashboardPage.tsx` | Orders today, revenue, active deliveries, low-stock alerts. |
| `src/pages/admin/AdminOrdersPage.tsx` | Order list, status updates and partner assignment. |
| `src/pages/admin/AdminProductsPage.tsx` | Catalogue and inventory management. |
| `src/pages/admin/AdminPaymentsPage.tsx` | Payment list, transaction history and refund action. |
| `src/pages/partner/PartnerDashboardPage.tsx` | Availability toggle, assigned orders, earnings summary. |
| `src/pages/partner/PartnerActiveDeliveryPage.tsx` | Active delivery steps: picked up, delivered, COD collection. |
| `src/pages/partner/PartnerHistoryPage.tsx` | Completed deliveries. |
| `src/components/ui/Button.tsx` | Styled button with variants and loading state. |
| `src/components/ui/Badge.tsx` | Status pill (order and payment statuses). |
| `src/components/ui/Modal.tsx` | Accessible modal dialog. |
| `src/components/ui/Drawer.tsx` | Side/bottom sheet used for cart, filters and order details. |
| `src/components/ui/Toast.tsx` | Toast notification component. |
| `src/components/ui/Spinner.tsx` | Loading indicator. |
| `src/components/ui/QuantityStepper.tsx` | +/- quantity control used on product cards and cart rows. |
| `src/components/layout/Header.tsx` | Top bar with logo, search and cart. |
| `src/components/layout/BottomNav.tsx` | Mobile bottom navigation. |
| `src/components/layout/Footer.tsx` | Footer links. |
| `src/components/layout/Logo.tsx` | QuickBasket wordmark. |
| `src/components/product/ProductCard.tsx` | Product tile with price, MRP and add button. |
| `src/components/product/ProductGrid.tsx` | Responsive grid of product cards. |
| `src/components/product/FilterBar.tsx` | Category, price and sort controls. |
| `src/components/product/CategoryTile.tsx` | Category tile used on the home page. |
| `src/components/cart/CartItemRow.tsx` | Single cart line. |
| `src/components/cart/BillSummary.tsx` | Subtotal, delivery fee and total as returned by the backend. |
| `src/components/checkout/AddressSelector.tsx` | Pick or add a delivery address. |
| `src/components/checkout/DeliverySlotPicker.tsx` | Delivery time slot selection. |
| `src/components/checkout/PaymentMethodSelector.tsx` | UPI apps, UPI QR, card, net banking, cash on delivery. |
| `src/components/checkout/ConfirmPaymentDialog.tsx` | Explicit customer confirmation of the exact amount before payment starts. |
| `src/components/payment/UpiQrCard.tsx` | Displays the backend-issued QR, exact amount and expiry. |
| `src/components/payment/ExpiryTimer.tsx` | Countdown to payment expiry. |
| `src/components/payment/PaymentStatusBanner.tsx` | Pending / success / failed / expired states from backend status only. |
| `src/components/payment/RazorpayCheckoutButton.tsx` | Opens Razorpay checkout using only the public key id returned by the backend. |
| `src/components/tracking/OrderTimeline.tsx` | Placed, packed, out for delivery, delivered. |
| `src/components/tracking/PartnerCard.tsx` | Delivery partner name and contact. |
| `src/components/assistant/AssistantPanel.tsx` | Chat panel container for the shopping assistant. |
| `src/components/assistant/ChatBubble.tsx` | Single chat message. |
| `src/components/assistant/SuggestedPrompts.tsx` | Quick prompt chips. |
| `src/components/assistant/ShoppingListCard.tsx` | Editable shopping list with 'Add all to cart'. Never starts a payment. |
| `src/components/admin/StatCard.tsx` | KPI tile. |
| `src/components/admin/OrdersTable.tsx` | Sortable orders table. |
| `src/components/admin/RefundDialog.tsx` | Refund amount and reason form (sends an Idempotency-Key). |
| `src/components/partner/DeliveryTaskCard.tsx` | Pickup/drop-off card with action buttons. |
| `src/components/partner/CodCollectionPrompt.tsx` | Prompts the partner to confirm cash collected. |
| `src/services/apiClient.ts` | Single HTTP client: base URL, bearer token, JSON handling, problem+json errors. |
| `src/services/authService.ts` | register, login, me. |
| `src/services/catalogService.ts` | categories, products, search. |
| `src/services/cartService.ts` | get/add/update/remove/clear cart. |
| `src/services/addressService.ts` | address CRUD. |
| `src/services/orderService.ts` | checkout, list, get, cancel orders. |
| `src/services/paymentService.ts` | initiate (with Idempotency-Key), verify, get status, list for order. |
| `src/services/adminService.ts` | admin orders, payments, refunds, partners. |
| `src/services/partnerService.ts` | assigned orders, pickup, deliver. |
| `src/services/assistantService.ts` | Shopping assistant calls (future). Must not call payment endpoints. |
| `src/hooks/useAuth.ts` | Access to AuthContext. |
| `src/hooks/useCart.ts` | Access to CartContext. |
| `src/hooks/useProducts.ts` | Fetch and cache product lists. |
| `src/hooks/useOrder.ts` | Fetch one order and poll its status. |
| `src/hooks/usePaymentPolling.ts` | Poll GET /api/payments/{id} until the backend reports a final status. |
| `src/hooks/useCountdown.ts` | Countdown helper for QR expiry. |
| `src/hooks/useDebounce.ts` | Debounce for search input. |
| `src/contexts/AuthContext.tsx` | Current user, token and role. |
| `src/contexts/CartContext.tsx` | Cart state mirrored from the backend. |
| `src/contexts/ToastContext.tsx` | Toast queue. |
| `src/contexts/AssistantContext.tsx` | Assistant panel open state and conversation. |
| `src/types/api.ts` | ProblemDetails, PagedResult and shared API types. |
| `src/types/auth.ts` | User, AuthResponse, login/register requests. |
| `src/types/catalog.ts` | Category and Product. |
| `src/types/cart.ts` | Cart and CartItem. |
| `src/types/order.ts` | Order, OrderItem, OrderEvent, statuses. |
| `src/types/payment.ts` | Payment, InitiatePaymentResponse, Refund, PaymentMethod and PaymentStatus unions. |
| `src/types/admin.ts` | Admin dashboard and partner types. |
| `src/types/assistant.ts` | Chat message and shopping-list types. |
| `src/utils/formatCurrency.ts` | INR formatting. |
| `src/utils/formatDate.ts` | Date/time formatting. |
| `src/utils/constants.ts` | Shared constants (brand name, tagline, polling intervals). |
| `public/.gitkeep` | Keeps the public folder in git until assets are added. |

## Rules for implementation
- Only `src/services/*` talks to the backend. Components never call `fetch` directly.
- The browser never holds Razorpay secrets. It receives only the public key id from `POST /api/payments/initiate`.
- Payment success is whatever the backend says (`GET /api/payments/{id}`), never a client-side callback or screenshot.
- The assistant feature may add items to the cart but must never call payment endpoints.
- Payments always require the explicit confirmation dialog (`ConfirmPaymentDialog`) and send `confirmedByCustomer: true`.
