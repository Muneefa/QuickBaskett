import { NavLink, Outlet } from "react-router-dom";
import { Logo } from "@/components/layout/Logo";
import { useAuth } from "@/hooks/useAuth";

const links: [string, string, boolean][] = [["/partner","Today",true],["/partner/active","Active delivery",false],["/partner/history","History",false]];

export function PartnerLayout() {
  const { user, logout } = useAuth();
  return (
    <div className="min-h-screen">
      <header className="border-b border-line bg-surface">
        <div className="mx-auto flex max-w-6xl flex-wrap items-center gap-3 px-4 py-3">
          <Logo to="/partner" />
          <span className="rounded-full bg-accent-soft px-2.5 py-1 text-xs font-extrabold text-accent">Delivery partner</span>
          <nav className="order-3 flex w-full gap-1 overflow-x-auto sm:order-none sm:w-auto" aria-label="Delivery partner">
            {links.map(([to, label, end]) => (
              <NavLink key={to} to={to} end={end} className={({ isActive }) => `whitespace-nowrap rounded-xl px-3 py-2 text-sm font-bold ${isActive ? "bg-brand-soft text-brand" : "text-muted hover:bg-surface2"}`}>{label}</NavLink>
            ))}
          </nav>
          <div className="ml-auto flex items-center gap-2 text-sm"><span className="hidden text-muted sm:inline">{user?.fullName}</span>
            <button onClick={logout} className="rounded-xl bg-surface2 px-3 py-2 font-bold">Sign out</button></div>
        </div>
      </header>
      <main className="mx-auto max-w-6xl px-4 py-4"><Outlet /></main>
    </div>
  );
}
