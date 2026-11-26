export interface Category { id: string; name: string; slug: string; imageUrl?: string | null; sortOrder: number; isActive: boolean }
export interface Product {
  id: string; categoryId: string; name: string; slug: string; description?: string | null; imageUrl?: string | null;
  price: number; mrpPrice?: number | null; unit: string; stockQuantity: number; isActive: boolean;
}
export interface CreateProductRequest { categoryId: string; name: string; description?: string; imageUrl?: string; price: number; mrpPrice?: number; unit: string; stockQuantity: number }
export interface UpdateProductRequest { name: string; description?: string | null; imageUrl?: string | null; price: number; mrpPrice?: number | null; unit: string; stockQuantity: number; isActive: boolean }
