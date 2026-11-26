import { useState, type FormEvent } from "react";
import { Link, useNavigate } from "react-router-dom";
import { Logo } from "./Logo";
import { useAuth } from "@/hooks/useAuth";
import { useCart } from "@/hooks/useCart";
import { paths } from "@/routes/routePaths";

export function Header() {
  const { user, logout } = useAuth();
  const { count } = useCart();
  const nav = useNavigate();
  const [q, setQ] = useState("");
  const customer = user?.role === "Customer";
  const submit = (e: FormEvent) => { e.preventDefault(); if (q.trim()) nav(`${paths.search}?q=${encodeURIComponent(q.trim())}`); };

  return (
    <header className="sticky top-0 z-30 border-b border-line bg-surface/95 backdrop-blur">
      <div className="mx-auto flex max-w-6xl flex-wrap items-center gap-3 px-4 py-3">
        <Logo />
        <form onSubmit={submit} role="search" className="order-3 w-full flex-1 md:order-none md:w-auto">
          <input value={q} onChange={e => setQ(e.target.value)} placeholder="Search milk, atta, bananas…" aria-label="Search products"
            className="h-11 w-full rounded-xl border border-line bg-surface2 px-4 text-sm outline-none focus:border-brand" />
        </form>
        <div className="ml-auto flex items-center gap-2">
          {customer ? (
            <>
              <Link to={paths.orders} className="hidden rounded-xl px-3 py-2 text-sm font-bold hover:bg-surface2 sm:block">Orders</Link>
              <Link to={paths.addresses} className="hidden rounded-xl px-3 py-2 text-sm font-bold hover:bg-surface2 sm:block">Addresses</Link>
              <button onClick={logout} className="rounded-xl px-3 py-2 text-sm font-bold text-muted hover:bg-surface2">Sign out</button>
            </>
          ) : (
            <Link to={paths.login} className="rounded-xl bg-surface2 px-4 py-2 text-sm font-bold">Sign in</Link>
          )}
          <Link to={paths.cart} className="relative grid h-11 w-11 place-items-center rounded-xl bg-brand text-brand-ink" aria-label={`Cart, ${count} items`}>
            🛒{count > 0 && <span className="absolute -right-1 -top-1 grid h-5 min-w-5 place-items-center rounded-full bg-accent px-1 text-xs font-extrabold text-white">{count}</span>}
          </Link>
        </div>
      </div>
    </header>
  );
}
