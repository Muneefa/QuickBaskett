import { useEffect, useState, type FormEvent } from "react";
import { Button } from "@/components/ui/Button";
import { Spinner, ErrorBox } from "@/components/ui/Spinner";
import { adminService } from "@/services/adminService";
import { catalogService } from "@/services/catalogService";
import { useToast } from "@/contexts/ToastContext";
import { formatCurrency } from "@/utils/formatCurrency";
import type { Category, Product } from "@/types/catalog";

export function AdminProductsPage() {
  const toast = useToast();
  const [products, setProducts] = useState<Product[] | null>(null);
  const [cats, setCats] = useState<Category[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [n, setN] = useState({ name: "", categoryId: "", price: "", unit: "", stock: "" });

  const load = () => Promise.all([catalogService.products({ pageSize: 100 }), catalogService.categories()])
    .then(([p, c]) => { setProducts(p.items); setCats(c); setN(v => ({ ...v, categoryId: v.categoryId || c[0]?.id || "" })); })
    .catch(e => setError(e instanceof Error ? e.message : "Could not load"));
  useEffect(() => { void load(); }, []);

  const update = async (p: Product, patch: Partial<Product>) => {
    const m = { ...p, ...patch };
    try { await adminService.updateProduct(p.id, { name: m.name, description: m.description, imageUrl: m.imageUrl, price: m.price, mrpPrice: m.mrpPrice, unit: m.unit, stockQuantity: m.stockQuantity, isActive: m.isActive }); toast.push("Saved"); await load(); }
    catch (e) { toast.push(e instanceof Error ? e.message : "Save failed", "err"); }
  };
  const create = async (e: FormEvent) => {
    e.preventDefault();
    try { await adminService.createProduct({ categoryId: n.categoryId, name: n.name, price: Number(n.price), unit: n.unit, stockQuantity: Number(n.stock) }); toast.push("Product added"); setN({ ...n, name: "", price: "", unit: "", stock: "" }); await load(); }
    catch (x) { toast.push(x instanceof Error ? x.message : "Could not add", "err"); }
  };
  if (error) return <ErrorBox message={error} />;
  if (!products) return <Spinner />;
  const f = "h-10 rounded-lg border border-line bg-surface2 px-2 text-sm";
  return (
    <>
      <h1 className="mb-3 text-2xl font-extrabold">Products</h1>
      <form onSubmit={e => void create(e)} className="mb-4 grid gap-2 rounded-2xl border border-line bg-surface p-3 sm:grid-cols-6">
        <input className={`${f} sm:col-span-2`} placeholder="Name" required value={n.name} onChange={e => setN({ ...n, name: e.target.value })} aria-label="Name" />
        <select className={f} value={n.categoryId} onChange={e => setN({ ...n, categoryId: e.target.value })} aria-label="Category">{cats.map(c => <option key={c.id} value={c.id}>{c.name}</option>)}</select>
        <input className={f} placeholder="Price" type="number" min="0.01" step="0.01" required value={n.price} onChange={e => setN({ ...n, price: e.target.value })} aria-label="Price" />
        <input className={f} placeholder="Unit (1 kg)" required value={n.unit} onChange={e => setN({ ...n, unit: e.target.value })} aria-label="Unit" />
        <input className={f} placeholder="Stock" type="number" min="0" required value={n.stock} onChange={e => setN({ ...n, stock: e.target.value })} aria-label="Stock" />
        <Button className="sm:col-span-6">Add product</Button>
      </form>
      <div className="overflow-x-auto rounded-2xl border border-line bg-surface">
        <table className="w-full min-w-[640px] text-left text-sm">
          <thead className="border-b border-line text-xs uppercase text-muted"><tr><th className="p-3">Product</th><th>Price</th><th>Stock</th><th>Active</th></tr></thead>
          <tbody>{products.map(p => (
            <tr key={p.id} className="border-b border-line last:border-0">
              <td className="p-3"><b>{p.name}</b><div className="text-xs text-muted">{p.unit}</div></td>
              <td><input aria-label={`Price for ${p.name}`} type="number" step="0.01" defaultValue={p.price} className={`${f} w-24`} onBlur={e => Number(e.target.value) !== p.price && Number(e.target.value) > 0 && void update(p, { price: Number(e.target.value) })} /><span className="ml-1 text-xs text-muted">{formatCurrency(p.price)}</span></td>
              <td><input aria-label={`Stock for ${p.name}`} type="number" min="0" defaultValue={p.stockQuantity} className={`${f} w-20`} onBlur={e => Number(e.target.value) !== p.stockQuantity && Number(e.target.value) >= 0 && void update(p, { stockQuantity: Number(e.target.value) })} /></td>
              <td><input aria-label={`${p.name} active`} type="checkbox" checked={p.isActive} onChange={e => void update(p, { isActive: e.target.checked })} /></td>
            </tr>))}</tbody>
        </table>
      </div>
      <p className="mt-2 text-xs text-muted">Price and stock save when you leave the field.</p>
    </>
  );
}
