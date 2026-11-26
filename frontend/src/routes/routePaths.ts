export const paths = {
  home: "/", login: "/login", register: "/register", adminLogin: "/admin/login", partnerLogin: "/partner/login",
  category: (id = ":id") => `/category/${id}`, search: "/search", product: (id = ":id") => `/product/${id}`,
  cart: "/cart", checkout: "/checkout", addresses: "/account/addresses", orders: "/orders",
  order: (id = ":id") => `/orders/${id}`, orderDone: (id = ":id") => `/orders/${id}/confirmed`,
  pay: (orderId = ":orderId") => `/pay/${orderId}`, payQr: (orderId = ":orderId") => `/pay/${orderId}/upi-qr`,
  admin: "/admin", adminOrders: "/admin/orders", adminProducts: "/admin/products", adminPayments: "/admin/payments",
  partner: "/partner", partnerActive: "/partner/active", partnerHistory: "/partner/history",
} as const;
