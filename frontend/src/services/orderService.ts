import { api } from "./apiClient";
import type { Order } from "@/types/order";

export const orderService = {
  checkout: (addressId: string, notes?: string) => api<Order>("/api/orders/checkout", { method: "POST", body: { addressId, notes } }),
  list: () => api<Order[]>("/api/orders"),
  get: (id: string) => api<Order>(`/api/orders/${id}`),
  cancel: (id: string) => api<Order>(`/api/orders/${id}/cancel`, { method: "POST" }),
};
