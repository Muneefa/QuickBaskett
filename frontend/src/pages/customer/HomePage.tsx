import { CategoryTile } from "@/components/product/CategoryTile";
import { ProductGrid } from "@/components/product/ProductGrid";
import { ErrorBox, Spinner } from "@/components/ui/Spinner";
import { useCatalog } from "@/hooks/useProducts";
import { FREE_DELIVERY_THRESHOLD, TAGLINE } from "@/utils/constants";
import { formatCurrency } from "@/utils/formatCurrency";

export function HomePage() {
  const { categories, products, loading, error, retry } = useCatalog();
  return (
    <>
      <section className="mb-5 rounded-2xl bg-brand p-6 text-brand-ink">
        <h1 className="text-3xl font-extrabold">{TAGLINE}</h1>
        <p className="mt-1 text-sm opacity-90">Free delivery on orders over {formatCurrency(FREE_DELIVERY_THRESHOLD)}.</p>
      </section>
      {loading ? <Spinner label="Loading the shop" /> : error ? <ErrorBox message={error} onRetry={retry} /> : (
        <>
          <h2 className="mb-2 text-xl font-extrabold">Shop by category</h2>
          <div className="mb-6 grid grid-cols-3 gap-2 sm:grid-cols-6">{categories.map(c => <CategoryTile key={c.id} category={c} />)}</div>
          <h2 className="mb-2 text-xl font-extrabold">Popular today</h2>
          <ProductGrid products={products.slice(0, 15)} />
        </>
      )}
    </>
  );
}
