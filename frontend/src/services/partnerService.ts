import { api } from "./apiClient";
import type { Order } from "@/types/order";

export const partnerService = {
  orders: () => api<Order[]>("/api/partner/orders"),
  pickup: (id: string) => api<Order>(`/api/partner/orders/${id}/pickup`, { method: "POST" }),
  deliver: (id: string, codCollected: boolean) => api<Order>(`/api/partner/orders/${id}/deliver`, { method: "POST", body: { codCollected } }),
};
