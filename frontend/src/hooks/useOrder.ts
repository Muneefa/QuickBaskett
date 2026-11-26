import { useCallback, useEffect, useState } from "react";
import { orderService } from "@/services/orderService";
import type { Order } from "@/types/order";

/** Loads one of the signed-in customer's orders and optionally refreshes it on an interval. */
export function useOrder(id: string | undefined, pollMs = 0) {
  const [order, setOrder] = useState<Order | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);

  const load = useCallback(async () => {
    if (!id) return;
    try { setOrder(await orderService.get(id)); setError(null); }
    catch (e) { setError(e instanceof Error ? e.message : "Could not load order"); }
    finally { setLoading(false); }
  }, [id]);

  useEffect(() => {
    void load();
    if (!pollMs) return;
    const t = setInterval(() => void load(), pollMs);
    return () => clearInterval(t);
  }, [load, pollMs]);

  return { order, error, loading, reload: load };
}
