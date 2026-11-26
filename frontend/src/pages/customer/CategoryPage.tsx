import { useState } from "react";
import { useParams } from "react-router-dom";
import { ProductGrid } from "@/components/product/ProductGrid";
import { FilterBar, sortProducts } from "@/components/product/FilterBar";
import { ErrorBox, Spinner } from "@/components/ui/Spinner";
import { useCatalog } from "@/hooks/useProducts";

export function CategoryPage() {
  const { id } = useParams();
  const { categories, products, loading, error, retry } = useCatalog();
  const [sort, setSort] = useState("relevance");
  if (loading) return <Spinner />;
  if (error) return <ErrorBox message={error} onRetry={retry} />;
  const cat = categories.find(c => c.id === id);
  return (
    <>
      <h1 className="mb-2 text-2xl font-extrabold">{cat?.name ?? "Category"}</h1>
      <FilterBar value={sort} onChange={setSort} />
      <ProductGrid products={sortProducts(products.filter(p => p.categoryId === id), sort)} empty="Nothing in this category yet." />
    </>
  );
}
