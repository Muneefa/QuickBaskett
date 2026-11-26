import { Link, useNavigate } from "react-router-dom";
import { QuantityStepper } from "@/components/ui/QuantityStepper";
import { useCart } from "@/hooks/useCart";
import { useAuth } from "@/hooks/useAuth";
import { formatCurrency } from "@/utils/formatCurrency";
import { productEmoji } from "@/utils/constants";
import { paths } from "@/routes/routePaths";
import type { Product } from "@/types/catalog";

export function ProductCard({ product: p }: { product: Product }) {
  const { qty, add, setQty } = useCart();
  const { user } = useAuth();
  const nav = useNavigate();
  const q = qty(p.id);
  const out = p.stockQuantity <= 0;
  const off = p.mrpPrice && p.mrpPrice > p.price ? Math.round((1 - p.price / p.mrpPrice) * 100) : 0;

  const addClick = () => {
    if (!user) { nav(paths.login, { state: { from: paths.home } }); return; }
    if (user.role !== "Customer") return;
    void add(p.id);
  };

  return (
    <article className="flex flex-col rounded-2xl border border-line bg-surface p-3">
      <Link to={paths.product(p.id)} className="mb-2 grid h-24 place-items-center rounded-xl bg-surface2 text-5xl" aria-label={p.name}>
        {p.imageUrl ? <img src={p.imageUrl} alt="" className="h-full w-full rounded-xl object-cover" loading="lazy" /> : productEmoji(p.name)}
      </Link>
      <Link to={paths.product(p.id)} className="text-sm font-bold leading-tight">{p.name}</Link>
      <span className="text-xs text-muted">{p.unit}</span>
      <div className="mt-auto flex items-end justify-between pt-2">
        <div>
          <b>{formatCurrency(p.price)}</b>
          {off > 0 && <div className="text-xs text-muted"><s>{formatCurrency(p.mrpPrice!)}</s> <span className="font-bold text-good">{off}% off</span></div>}
        </div>
        {out ? <span className="text-xs font-bold text-bad">Out of stock</span>
          : q > 0 ? <QuantityStepper value={q} label={p.name} onChange={n => void setQty(p.id, Math.min(n, p.stockQuantity))} />
          : <button onClick={addClick} className="h-10 rounded-xl border-2 border-brand px-4 text-sm font-extrabold text-brand" aria-label={`Add ${p.name}`}>Add</button>}
      </div>
    </article>
  );
}
