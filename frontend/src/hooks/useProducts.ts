import { useEffect, useState } from "react";
import { catalogService } from "@/services/catalogService";
import type { Category, Product } from "@/types/catalog";

export function useCatalog() {
  const [categories, setCategories] = useState<Category[]>([]);
  const [products, setProducts] = useState<Product[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [tick, setTick] = useState(0);

  useEffect(() => {
    let live = true;
    setLoading(true); setError(null);
    Promise.all([catalogService.categories(), catalogService.products({ pageSize: 100 })])
      .then(([c, p]) => { if (live) { setCategories(c); setProducts(p.items.filter(x => x.isActive)); } })
      .catch(e => live && setError(e instanceof Error ? e.message : "Could not load products"))
      .finally(() => live && setLoading(false));
    return () => { live = false; };
  }, [tick]);

  return { categories, products, loading, error, retry: () => setTick(t => t + 1) };
}
