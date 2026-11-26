import { api } from "./apiClient";
import type { Address, SaveAddressRequest } from "@/types/cart";

export const addressService = {
  list: () => api<Address[]>("/api/addresses"),
  create: (b: SaveAddressRequest) => api<Address>("/api/addresses", { method: "POST", body: b }),
  update: (id: string, b: SaveAddressRequest) => api<Address>(`/api/addresses/${id}`, { method: "PUT", body: b }),
  remove: (id: string) => api<void>(`/api/addresses/${id}`, { method: "DELETE" }),
};
