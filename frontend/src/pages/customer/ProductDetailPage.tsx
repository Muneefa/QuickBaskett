import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { QuantityStepper } from "@/components/ui/QuantityStepper";
import { Button } from "@/components/ui/Button";
import { ErrorBox, Spinner } from "@/components/ui/Spinner";
import { catalogService } from "@/services/catalogService";
import { useCart } from "@/hooks/useCart";
import { useAuth } from "@/hooks/useAuth";
import { formatCurrency } from "@/utils/formatCurrency";
import { productEmoji } from "@/utils/constants";
import { paths } from "@/routes/routePaths";
import type { Product } from "@/types/catalog";

export function ProductDetailPage() {
  const { id } = useParams();
  const nav = useNavigate();
  const { user } = useAuth();
  const { qty, add, setQty } = useCart();
  const [p, setP] = useState<Product | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => { if (id) catalogService.product(id).then(setP).catch(e => setError(e instanceof Error ? e.message : "Not found")); }, [id]);
  if (error) return <ErrorBox message={error} />;
  if (!p) return <Spinner />;
  const q = qty(p.id);
  return (
    <div className="grid gap-6 md:grid-cols-2">
      <div className="grid h-64 place-items-center rounded-2xl bg-surface2 text-8xl">
        {p.imageUrl ? <img src={p.imageUrl} alt={p.name} className="h-full w-full rounded-2xl object-cover" /> : productEmoji(p.name)}
      </div>
      <div>
        <h1 className="text-3xl font-extrabold">{p.name}</h1>
        <p className="text-muted">{p.unit}</p>
        <p className="my-3 text-2xl font-extrabold">{formatCurrency(p.price)} {p.mrpPrice && p.mrpPrice > p.price && <s className="text-base font-normal text-muted">{formatCurrency(p.mrpPrice)}</s>}</p>
        {p.description && <p className="mb-4 text-sm">{p.description}</p>}
        {p.stockQuantity <= 0 ? <p className="font-bold text-bad">Out of stock</p>
          : q > 0 ? <QuantityStepper value={q} label={p.name} onChange={n => void setQty(p.id, Math.min(n, p.stockQuantity))} />
          : <Button onClick={() => user ? (user.role === "Customer" && void add(p.id)) : nav(paths.login, { state: { from: paths.product(p.id) } })}>Add to cart</Button>}
      </div>
    </div>
  );
}
