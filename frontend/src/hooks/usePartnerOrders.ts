import { useCallback, useEffect, useState } from "react";
import { partnerService } from "@/services/partnerService";
import type { Order } from "@/types/order";

export function usePartnerOrders() {
  const [orders, setOrders] = useState<Order[] | null>(null);
  const [error, setError] = useState<string | null>(null);
  const reload = useCallback(() => partnerService.orders().then(o => { setOrders(o); setError(null); }).catch(e => setError(e instanceof Error ? e.message : "Could not load")), []);
  useEffect(() => { void reload(); const t = setInterval(() => void reload(), 15000); return () => clearInterval(t); }, [reload]);
  return { orders, error, reload };
}
