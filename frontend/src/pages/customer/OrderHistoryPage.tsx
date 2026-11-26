import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { Badge } from "@/components/ui/Badge";
import { Spinner, ErrorBox } from "@/components/ui/Spinner";
import { orderService } from "@/services/orderService";
import { formatCurrency } from "@/utils/formatCurrency";
import { formatDateTime } from "@/utils/formatDate";
import { paths } from "@/routes/routePaths";
import type { Order } from "@/types/order";

export function OrderHistoryPage() {
  const [orders, setOrders] = useState<Order[] | null>(null);
  const [error, setError] = useState<string | null>(null);
  useEffect(() => { orderService.list().then(setOrders).catch(e => setError(e instanceof Error ? e.message : "Could not load orders")); }, []);
  if (error) return <ErrorBox message={error} />;
  if (!orders) return <Spinner />;
  return (
    <>
      <h1 className="mb-3 text-2xl font-extrabold">Your orders</h1>
      {!orders.length ? <p className="text-muted">No orders yet. <Link className="font-bold text-brand" to={paths.home}>Start shopping</Link></p> : (
        <ul className="space-y-2">{orders.map(o => (
          <li key={o.id}><Link to={paths.order(o.id)} className="flex items-center justify-between rounded-xl border border-line bg-surface p-3">
            <div><b>{o.orderNumber}</b><div className="text-xs text-muted">{formatDateTime(o.createdAtUtc)} · {o.items.length} items</div></div>
            <div className="text-right"><b>{formatCurrency(o.totalAmount)}</b><div><Badge status={o.status} /></div></div>
          </Link></li>))}</ul>
      )}
    </>
  );
}
