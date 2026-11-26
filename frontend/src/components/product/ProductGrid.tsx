import { ProductCard } from "./ProductCard";
import type { Product } from "@/types/catalog";

export function ProductGrid({ products, empty = "No products found." }: { products: Product[]; empty?: string }) {
  if (!products.length) return <p className="p-6 text-center text-muted">{empty}</p>;
  return <div className="grid grid-cols-2 gap-3 sm:grid-cols-3 lg:grid-cols-5">{products.map(p => <ProductCard key={p.id} product={p} />)}</div>;
}
