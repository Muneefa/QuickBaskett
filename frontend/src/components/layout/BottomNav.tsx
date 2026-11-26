import { NavLink } from "react-router-dom";
import { paths } from "@/routes/routePaths";
import { useCart } from "@/hooks/useCart";

export function BottomNav() {
  const { count } = useCart();
  const item = ({ isActive }: { isActive: boolean }) =>
    `flex flex-1 flex-col items-center gap-0.5 py-2 text-xs font-bold ${isActive ? "text-brand" : "text-muted"}`;
  return (
    <nav className="fixed inset-x-0 bottom-0 z-30 flex border-t border-line bg-surface sm:hidden" aria-label="Main">
      <NavLink to={paths.home} end className={item}><span>🏠</span>Home</NavLink>
      <NavLink to={paths.search} className={item}><span>🔎</span>Search</NavLink>
      <NavLink to={paths.cart} className={item}><span>🛒</span>Cart{count > 0 ? ` (${count})` : ""}</NavLink>
      <NavLink to={paths.orders} className={item}><span>📦</span>Orders</NavLink>
    </nav>
  );
}
