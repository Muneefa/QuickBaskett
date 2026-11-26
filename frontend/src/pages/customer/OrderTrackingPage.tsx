import { Link, useParams } from "react-router-dom";
import { OrderTimeline } from "@/components/tracking/OrderTimeline";
import { PartnerCard } from "@/components/tracking/PartnerCard";
import { Badge } from "@/components/ui/Badge";
import { Button } from "@/components/ui/Button";
import { Spinner, ErrorBox } from "@/components/ui/Spinner";
import { useOrder } from "@/hooks/useOrder";
import { orderService } from "@/services/orderService";
import { useToast } from "@/contexts/ToastContext";
import { formatCurrency } from "@/utils/formatCurrency";
import { paths } from "@/routes/routePaths";

export function OrderTrackingPage() {
  const { id } = useParams();
  const toast = useToast();
  const { order, error, loading, reload } = useOrder(id, 8000);
  if (loading) return <Spinner />;
  if (error || !order) return <ErrorBox message={error ?? "Order not found"} />;
  const cancellable = order.status === "PendingPayment" || order.status === "Confirmed";
  const cancel = async () => { try { await orderService.cancel(order.id); toast.push("Order cancelled"); await reload(); } catch (e) { toast.push(e instanceof Error ? e.message : "Could not cancel", "err"); } };
  return (
    <div className="grid gap-6 md:grid-cols-2">
      <section className="space-y-4">
        <div className="flex items-center justify-between"><h1 className="text-2xl font-extrabold">{order.orderNumber}</h1><Badge status={order.status} /></div>
        <OrderTimeline status={order.status} events={order.events} />
        <PartnerCard partner={order.deliveryPartner} />
        {order.status === "PendingPayment" && <Link to={paths.pay(order.id)}><Button className="w-full">Complete payment</Button></Link>}
        {cancellable && <Button variant="danger" className="w-full" onClick={() => void cancel()}>Cancel order</Button>}
      </section>
      <section className="space-y-3 rounded-2xl border border-line bg-surface p-4">
        <h2 className="font-extrabold">Items</h2>
        <ul className="divide-y divide-line text-sm">{order.items.map(i => <li key={i.productId} className="flex justify-between py-2"><span>{i.productName} × {i.quantity}</span><b>{formatCurrency(i.lineTotal)}</b></li>)}</ul>
        <div className="flex justify-between border-t border-line pt-2 font-extrabold"><span>Total</span><span>{formatCurrency(order.totalAmount)}</span></div>
        <p className="text-xs text-muted">Delivering to {order.deliveryRecipient}, {order.deliveryAddress}</p>
        {order.latestPaymentStatus && <p className="text-xs text-muted">Payment: {order.latestPaymentStatus}</p>}
      </section>
    </div>
  );
}
