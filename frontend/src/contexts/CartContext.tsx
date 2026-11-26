import { createContext, useCallback, useContext, useEffect, useMemo, useState, type ReactNode } from "react";
import { cartService } from "@/services/cartService";
import { useAuthContext } from "./AuthContext";
import { useToast } from "./ToastContext";
import type { Cart } from "@/types/cart";

const EMPTY: Cart = { items: [], subtotal: 0, deliveryFee: 0, total: 0, currency: "INR" };

interface CartApi {
  cart: Cart;
  count: number;
  qty: (productId: string) => number;
  setQty: (productId: string, quantity: number) => Promise<void>;
  add: (productId: string, quantity?: number) => Promise<void>;
  clear: () => Promise<void>;
  refresh: () => Promise<void>;
}
const Ctx = createContext<CartApi | null>(null);

export function CartProvider({ children }: { children: ReactNode }) {
  const { user } = useAuthContext();
  const toast = useToast();
  const [cart, setCart] = useState<Cart>(EMPTY);
  const isCustomer = user?.role === "Customer";

  const refresh = useCallback(async () => {
    if (!isCustomer) { setCart(EMPTY); return; }
    try { setCart(await cartService.get()); } catch { /* shown by the page that needs it */ }
  }, [isCustomer]);

  useEffect(() => { void refresh(); }, [refresh]);

  const run = useCallback(async (fn: () => Promise<Cart>) => {
    try { setCart(await fn()); } catch (e) { toast.push(e instanceof Error ? e.message : "Cart update failed", "err"); }
  }, [toast]);

  const value = useMemo<CartApi>(() => ({
    cart,
    count: cart.items.reduce((n, i) => n + i.quantity, 0),
    qty: id => cart.items.find(i => i.productId === id)?.quantity ?? 0,
    setQty: (id, q) => run(() => (q <= 0 ? cartService.remove(id) : cartService.setQty(id, q))),
    add: (id, q = 1) => {
      const cur = cart.items.find(i => i.productId === id)?.quantity ?? 0;
      return run(() => (cur ? cartService.setQty(id, cur + q) : cartService.add(id, q)));
    },
    clear: async () => { await run(() => cartService.clear()); },
    refresh,
  }), [cart, run, refresh]);

  return <Ctx.Provider value={value}>{children}</Ctx.Provider>;
}
export function useCartContext(): CartApi {
  const c = useContext(Ctx);
  if (!c) throw new Error("useCart outside CartProvider");
  return c;
}
