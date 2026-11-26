import { useEffect, useState } from "react";
import { useSearchParams } from "react-router-dom";
import { ProductGrid } from "@/components/product/ProductGrid";
import { FilterBar, sortProducts } from "@/components/product/FilterBar";
import { ErrorBox, Spinner } from "@/components/ui/Spinner";
import { catalogService } from "@/services/catalogService";
import type { Product } from "@/types/catalog";

export function SearchPage() {
  const [params] = useSearchParams();
  const q = params.get("q") ?? "";
  const [items, setItems] = useState<Product[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [sort, setSort] = useState("relevance");

  useEffect(() => {
    if (!q) { setItems([]); return; }
    let live = true; setLoading(true); setError(null);
    catalogService.products({ search: q }).then(r => live && setItems(r.items.filter(p => p.isActive)))
      .catch(e => live && setError(e instanceof Error ? e.message : "Search failed")).finally(() => live && setLoading(false));
    return () => { live = false; };
  }, [q]);

  return (
    <>
      <h1 className="mb-2 text-2xl font-extrabold">{q ? `Results for “${q}”` : "Search"}</h1>
      {!q && <p className="text-muted">Type in the search box above.</p>}
      {loading ? <Spinner label="Searching" /> : error ? <ErrorBox message={error} /> : q ? (<><FilterBar value={sort} onChange={setSort} /><ProductGrid products={sortProducts(items, sort)} empty={`No results for “${q}”.`} /></>) : null}
    </>
  );
}
