import { api } from "./apiClient";
import type { Cart } from "@/types/cart";

export const cartService = {
  get: () => api<Cart>("/api/cart"),
  add: (productId: string, quantity: number) => api<Cart>("/api/cart/items", { method: "POST", body: { productId, quantity } }),
  setQty: (productId: string, quantity: number) => api<Cart>(`/api/cart/items/${productId}`, { method: "PUT", body: { quantity } }),
  remove: (productId: string) => api<Cart>(`/api/cart/items/${productId}`, { method: "DELETE" }),
  clear: () => api<Cart>("/api/cart", { method: "DELETE" }),
};
