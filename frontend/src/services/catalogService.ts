import { api } from "./apiClient";
import type { PagedResult } from "@/types/api";
import type { Category, Product } from "@/types/catalog";

export const catalogService = {
  categories: () => api<Category[]>("/api/categories", { auth: false }),
  products: (q: { categoryId?: string; search?: string; page?: number; pageSize?: number } = {}) => {
    const p = new URLSearchParams();
    if (q.categoryId) p.set("categoryId", q.categoryId);
    if (q.search) p.set("search", q.search);
    p.set("page", String(q.page ?? 1));
    p.set("pageSize", String(q.pageSize ?? 100));
    return api<PagedResult<Product>>(`/api/products?${p}`, { auth: false });
  },
  product: (id: string) => api<Product>(`/api/products/${id}`, { auth: false }),
};
