import { api, newIdempotencyKey } from "./apiClient";
import type { PagedResult } from "@/types/api";
import type { Order } from "@/types/order";
import type { Payment } from "@/types/payment";
import type { Refund } from "@/types/payment";
import type { User } from "@/types/auth";
import type { CreateProductRequest, Product, UpdateProductRequest } from "@/types/catalog";

export const adminService = {
  orders: (status?: string, page = 1, pageSize = 50) =>
    api<PagedResult<Order>>(`/api/admin/orders?${new URLSearchParams({ ...(status ? { status } : {}), page: String(page), pageSize: String(pageSize) })}`),
  setStatus: (id: string, status: string, note?: string) => api<Order>(`/api/admin/orders/${id}/status`, { method: "PUT", body: { status, note } }),
  assign: (id: string, partnerId: string) => api<Order>(`/api/admin/orders/${id}/assign`, { method: "PUT", body: { partnerId } }),
  payments: (page = 1, pageSize = 50) => api<PagedResult<Payment>>(`/api/admin/payments?page=${page}&pageSize=${pageSize}`),
  refund: (paymentId: string, amount: number, reason: string, key = newIdempotencyKey()) =>
    api<Refund>(`/api/admin/payments/${paymentId}/refunds`, { method: "POST", body: { amount, reason }, headers: { "Idempotency-Key": key } }),
  partners: () => api<User[]>("/api/admin/partners"),
  createProduct: (b: CreateProductRequest) => api<Product>("/api/admin/products", { method: "POST", body: b }),
  updateProduct: (id: string, b: UpdateProductRequest) => api<Product>(`/api/admin/products/${id}`, { method: "PUT", body: b }),
  devProcessRefund: (refundId: string) => api<void>(`/api/dev/mock-payments/refunds/${refundId}/process`, { method: "POST" }),
};
