import { Spinner, ErrorBox } from "@/components/ui/Spinner";
import { usePartnerOrders } from "@/hooks/usePartnerOrders";
import { formatCurrency } from "@/utils/formatCurrency";
import { formatDateTime } from "@/utils/formatDate";

export function PartnerHistoryPage() {
  const { orders, error } = usePartnerOrders();
  if (error) return <ErrorBox message={error} />;
  if (!orders) return <Spinner />;
  const done = orders.filter(o => o.status === "Delivered");
  return (
    <>
      <h1 className="mb-3 text-2xl font-extrabold">Delivery history</h1>
      {!done.length ? <p className="text-muted">No completed deliveries yet.</p> : (
        <ul className="space-y-2">{done.map(o => <li key={o.id} className="flex justify-between rounded-xl border border-line bg-surface p-3 text-sm"><span><b>{o.orderNumber}</b> · {o.deliveryRecipient}<br /><span className="text-xs text-muted">{formatDateTime(o.deliveredAtUtc)}</span></span><b>{formatCurrency(o.totalAmount)}</b></li>)}</ul>)}
    </>
  );
}
