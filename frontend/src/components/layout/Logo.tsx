import { Link } from "react-router-dom";
export function Logo({ to = "/", light = false }: { to?: string; light?: boolean }) {
  return (
    <Link to={to} className="inline-flex items-center gap-2 font-display text-xl font-extrabold" aria-label="QuickBasket home">
      <span className="grid h-9 w-9 place-items-center rounded-xl bg-brand text-brand-ink">🧺</span>
      <span className={light ? "text-brand-ink" : "text-ink"}>Quick<span className="text-accent">Basket</span></span>
    </Link>
  );
}
