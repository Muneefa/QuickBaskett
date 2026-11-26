import { useCallback, useEffect, useState } from "react";
import { OrdersTable } from "@/components/admin/OrdersTable";
import { Spinner, ErrorBox } from "@/components/ui/Spinner";
import { adminService } from "@/services/adminService";
import { useToast } from "@/contexts/ToastContext";
import type { Order, OrderStatus } from "@/types/order";
import type { User } from "@/types/auth";

const FILTERS = ["", "PendingPayment", "Confirmed", "Packed", "OutForDelivery", "Delivered", "Cancelled"];

export function AdminOrdersPage() {
  const toast = useToast();
  const [orders, setOrders] = useState<Order[] | null>(null);
  const [partners, setPartners] = useState<User[]>([]);
  const [status, setStatus] = useState("");
  const [error, setError] = useState<string | null>(null);

  const load = useCallback(() => adminService.orders(status || undefined).then(r => setOrders(r.items)).catch(e => setError(e instanceof Error ? e.message : "Could not load")), [status]);
  useEffect(() => { void load(); adminService.partners().then(setPartners).catch(() => {}); }, [load]);

  const act = async (fn: () => Promise<unknown>, ok: string) => { try { await fn(); toast.push(ok); await load(); } catch (e) { toast.push(e instanceof Error ? e.message : "Action failed", "err"); } };
  if (error) return <ErrorBox message={error} />;
  return (
    <>
      <div className="mb-3 flex flex-wrap items-center justify-between gap-2">
        <h1 className="text-2xl font-extrabold">Orders</h1>
        <select aria-label="Filter by status" value={status} onChange={e => setStatus(e.target.value)} className="h-10 rounded-xl border border-line bg-surface px-3 text-sm">
          {FILTERS.map(f => <option key={f} value={f}>{f || "All statuses"}</option>)}</select>
      </div>
      {!orders ? <Spinner /> : <OrdersTable orders={orders} partners={partners}
        onStatus={(o, s: OrderStatus) => void act(() => adminService.setStatus(o.id, s), `Order moved to ${s}`)}
        onAssign={(o, pid) => void act(() => adminService.assign(o.id, pid), "Partner assigned")} />}
    </>
  );
}
